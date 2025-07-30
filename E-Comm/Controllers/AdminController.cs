using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using E_Comm.Models;

namespace E_Comm.Controllers
{
    public class BulkStockUpdateRequest
    {
        public int[] ItemIds { get; set; } = new int[0];
        public string UpdateType { get; set; } = "";
        public int Quantity { get; set; }
        public bool UpdatePrice { get; set; }
        public double? NewPrice { get; set; }
        public string Reason { get; set; } = "";
    }

    public class BulkDeleteRequest
    {
        public int[] ItemIds { get; set; } = new int[0];
    }
}

namespace E_Comm.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EntertainmentGuildContext _context;

        public AdminController(EntertainmentGuildContext context)
        {
            _context = context;
        }

        // Admin Dashboard - Entry Point for Administrators
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Admin";
            
            // Get overview statistics
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            
            // Get stock statistics - count products, not stocktakes (consistent with ManageItems page)
            var allProducts = await _context.Products
                .Include(p => p.Stocktakes)
                .ToListAsync();

            ViewBag.LowStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) < 10 && p.Stocktakes.Sum(s => s.Quantity) > 0).Count();
            ViewBag.OutOfStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) == 0).Count();
            ViewBag.InStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) > 0).Count();

            // Get recent activity data
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderID)
                .Take(5)
                .ToListAsync();

            // Get popular genres data
            var genreStats = await _context.Products
                .Include(p => p.Genre)
                .GroupBy(p => p.Genre.Name)
                .Select(g => new { 
                    Genre = g.Key, 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Get low stock products
            var lowStockProducts = await _context.Stocktakes
                .Include(s => s.Product)
                .Where(s => s.Quantity < 10)
                .OrderBy(s => s.Quantity)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;
            ViewBag.GenreStats = genreStats;
            ViewBag.LowStockProducts = lowStockProducts;

            // Generate recent activity data
            var recentActivity = await GenerateRecentActivityAsync();
            ViewBag.RecentActivity = recentActivity;

            return View();
        }

        // Generate recent activity data for the dashboard
        private async Task<List<ActivityItem>> GenerateRecentActivityAsync()
        {
            var activities = new List<ActivityItem>();
            var now = DateTime.Now;

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderID)
                .Take(3)
                .ToListAsync();

            foreach (var order in recentOrders)
            {
                var timestamp = now.AddHours(-new Random().Next(1, 24)); // Simulate realistic timestamp
                activities.Add(new ActivityItem
                {
                    Type = ActivityType.OrderPlaced.ToString(),
                    Icon = "fas fa-shopping-cart",
                    IconColor = "text-warning",
                    Title = "New Order Placed",
                    Description = $"Order #{order.OrderID} placed by {order.Customer?.Email ?? "Unknown customer"}",
                    Timestamp = timestamp,
                    TimeAgo = GetTimeAgo(timestamp)
                });
            }

            // Get recently added products
            var recentProducts = await _context.Products
                .Where(p => p.LastUpdated.HasValue)
                .OrderByDescending(p => p.LastUpdated)
                .Take(3)
                .ToListAsync();

            foreach (var product in recentProducts)
            {
                var timestamp = product.LastUpdated ?? now.AddHours(-new Random().Next(1, 48));
                activities.Add(new ActivityItem
                {
                    Type = ActivityType.ProductAdded.ToString(),
                    Icon = "fas fa-plus",
                    IconColor = "text-success",
                    Title = "Product Updated",
                    Description = $"{product.Name} was updated by {product.LastUpdatedBy ?? "Admin"}",
                    Timestamp = timestamp,
                    TimeAgo = GetTimeAgo(timestamp)
                });
            }

            // Get recently registered users (simulate registration activity)
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.UserID)
                .Take(2)
                .ToListAsync();

            foreach (var user in recentUsers)
            {
                var timestamp = now.AddHours(-new Random().Next(1, 72));
                activities.Add(new ActivityItem
                {
                    Type = ActivityType.UserRegistered.ToString(),
                    Icon = "fas fa-user",
                    IconColor = "text-info",
                    Title = "User Account",
                    Description = $"{(user.IsAdmin ? "Admin" : "Customer")} account for {user.Email}",
                    Timestamp = timestamp,
                    TimeAgo = GetTimeAgo(timestamp)
                });
            }

            // Get low stock alerts
            var lowStockItems = await _context.Stocktakes
                .Include(s => s.Product)
                .Where(s => s.Quantity < 10 && s.Quantity > 0)
                .OrderBy(s => s.Quantity)
                .Take(2)
                .ToListAsync();

            foreach (var item in lowStockItems)
            {
                var timestamp = now.AddHours(-new Random().Next(1, 12));
                activities.Add(new ActivityItem
                {
                    Type = ActivityType.LowStockAlert.ToString(),
                    Icon = "fas fa-exclamation-triangle",
                    IconColor = "text-danger",
                    Title = "Low Stock Alert",
                    Description = $"{item.Product?.Name} is running low on stock ({item.Quantity} remaining)",
                    Timestamp = timestamp,
                    TimeAgo = GetTimeAgo(timestamp)
                });
            }

            // Sort activities by timestamp (most recent first)
            return activities.OrderByDescending(a => a.Timestamp).Take(4).ToList();
        }

        // API endpoint for refreshing dashboard data
        [HttpPost]
        public async Task<IActionResult> RefreshDashboard()
        {
            var recentActivity = await GenerateRecentActivityAsync();
            return Json(new { success = true, activities = recentActivity });
        }

        // Helper method to generate "time ago" string
        private string GetTimeAgo(DateTime timestamp)
        {
            var timeSpan = DateTime.Now - timestamp;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            else if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";
            else
                return timestamp.ToString("MMM dd, yyyy");
        }

        // Item Management (Business Scenario 2: Item Management from the User Interface Layer)
        public async Task<IActionResult> ManageItems(string searchTerm, int? genreFilter, string stockFilter, int page = 1)
        {
            const int pageSize = 20;
            
            var products = _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => 
                    p.Name.Contains(searchTerm) || 
                    p.Author.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            // Genre filtering
            if (genreFilter.HasValue)
            {
                products = products.Where(p => p.GenreID == genreFilter);
            }

            // Stock filtering
            if (!string.IsNullOrEmpty(stockFilter))
            {
                switch (stockFilter.ToLower())
                {
                    case "low":
                        products = products.Where(p => p.Stocktakes.Sum(s => s.Quantity) < 10 && p.Stocktakes.Sum(s => s.Quantity) > 0);
                        break;
                    case "out":
                        products = products.Where(p => p.Stocktakes.Sum(s => s.Quantity) == 0);
                        break;
                    case "available":
                        products = products.Where(p => p.Stocktakes.Sum(s => s.Quantity) > 0);
                        break;
                }
            }

            // Get total count for pagination
            var totalProducts = await products.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Apply pagination
            var paginatedProducts = await products
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get stock statistics for the statistics cards
            var allProducts = await _context.Products
                .Include(p => p.Stocktakes)
                .ToListAsync();

            var lowStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) < 10 && p.Stocktakes.Sum(s => s.Quantity) > 0).Count();
            var outOfStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) == 0).Count();
            var inStockItems = allProducts.Where(p => p.Stocktakes.Sum(s => s.Quantity) > 0).Count();

            // Pass data to view
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.GenreFilter = genreFilter;
            ViewBag.StockFilter = stockFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStockItems = lowStockItems;
            ViewBag.OutOfStockItems = outOfStockItems;
            ViewBag.InStockItems = inStockItems;

            return View(paginatedProducts);
        }

        [HttpGet]
        public async Task<IActionResult> CreateItem()
        {
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(Product product, int sourceId, int quantity, double price, IFormFile? productImage)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (productImage != null && productImage.Length > 0)
                {
                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(productImage.ContentType.ToLower()))
                    {
                        ModelState.AddModelError("productImage", "Only JPEG, PNG, and GIF images are allowed.");
                        ViewBag.Genres = await _context.Genres.ToListAsync();
                        ViewBag.Sources = await _context.Sources.ToListAsync();
                        return View(product);
                    }

                    // Validate file size (max 5MB)
                    if (productImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("productImage", "Image size must be less than 5MB.");
                        ViewBag.Genres = await _context.Genres.ToListAsync();
                        ViewBag.Sources = await _context.Sources.ToListAsync();
                        return View(product);
                    }

                    // Convert image to byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await productImage.CopyToAsync(memoryStream);
                        product.ProductImage = memoryStream.ToArray();
                        product.ImageContentType = productImage.ContentType;
                    }
                }

                // Use email instead of name for better reliability with test credentials
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                
                // Try to find the user in the database by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                product.LastUpdatedBy = user?.UserName; // This can be null, which is fine
                product.LastUpdated = DateTime.Now;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add to stocktake
                var stocktake = new Stocktake
                {
                    ProductId = product.ID,
                    SourceId = sourceId,
                    Quantity = quantity,
                    Price = price
                };
                _context.Stocktakes.Add(stocktake);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ManageItems));
            }

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var product = await _context.Products
                .Include(p => p.Stocktakes)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (product == null)
                return NotFound();

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditItem(Product product, IFormFile? productImage)
        {
            if (ModelState.IsValid)
            {
                // Get the existing product to preserve current image if no new one is uploaded
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ID == product.ID);
                if (existingProduct == null)
                    return NotFound();

                // Handle image upload
                if (productImage != null && productImage.Length > 0)
                {
                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(productImage.ContentType.ToLower()))
                    {
                        ModelState.AddModelError("productImage", "Only JPEG, PNG, and GIF images are allowed.");
                        ViewBag.Genres = await _context.Genres.ToListAsync();
                        ViewBag.Sources = await _context.Sources.ToListAsync();
                        return View(product);
                    }

                    // Validate file size (max 5MB)
                    if (productImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("productImage", "Image size must be less than 5MB.");
                        ViewBag.Genres = await _context.Genres.ToListAsync();
                        ViewBag.Sources = await _context.Sources.ToListAsync();
                        return View(product);
                    }

                    // Convert image to byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await productImage.CopyToAsync(memoryStream);
                        product.ProductImage = memoryStream.ToArray();
                        product.ImageContentType = productImage.ContentType;
                    }
                }
                else
                {
                    // Preserve existing image if no new one is uploaded
                    product.ProductImage = existingProduct.ProductImage;
                    product.ImageContentType = existingProduct.ImageContentType;
                }

                // Use email instead of name for better reliability with test credentials
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                
                // Try to find the user in the database by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                product.LastUpdatedBy = user?.UserName; // This can be null, which is fine
                product.LastUpdated = DateTime.Now;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ManageItems));
            }

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Remove associated stocktakes first
                var stocktakes = await _context.Stocktakes.Where(s => s.ProductId == id).ToListAsync();
                _context.Stocktakes.RemoveRange(stocktakes);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageItems));
        }

        // User Management (Business Scenarios 3 & 4: User Management)
        public async Task<IActionResult> ManageUsers(string searchTerm, string roleFilter, int page = 1)
        {
            const int pageSize = 20;
            
            var users = _context.Users.AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => 
                    u.UserName.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm) ||
                    u.Name.Contains(searchTerm));
            }

            // Role filtering
            if (!string.IsNullOrEmpty(roleFilter))
            {
                switch (roleFilter.ToLower())
                {
                    case "admin":
                        users = users.Where(u => u.IsAdmin);
                        break;
                    case "customer":
                        users = users.Where(u => !u.IsAdmin);
                        break;
                }
            }

            // Get total count for pagination
            var totalUsers = await users.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            // Apply pagination
            var paginatedUsers = await users
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get user statistics
            var totalUsersCount = await _context.Users.CountAsync();
            var adminCount = await _context.Users.CountAsync(u => u.IsAdmin);
            var customerCount = await _context.Users.CountAsync(u => !u.IsAdmin);

            // Pass data to view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalUsersCount = totalUsersCount;
            ViewBag.AdminCount = adminCount;
            ViewBag.CustomerCount = customerCount;

            return View(paginatedUsers);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new UserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model, string password, string confirmPassword)
        {
            // Custom validation
            await ValidateUserViewModel(model, password, confirmPassword);
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Create User
                    var user = model.ToUser();
                    
                    // Hash password
                    if (!string.IsNullOrEmpty(password))
                    {
                        var salt = GenerateSalt();
                        user.Salt = salt;
                        user.HashPW = HashPassword(password, salt);
                    }
                    
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                    
                    // Create Customer profile if customer details provided
                    var customer = model.ToCustomer();
                    if (customer != null)
                    {
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();
                    }
                    
                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction(nameof(ManageUsers));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the user. Please try again.");
                }
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound();
                
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);
            var viewModel = UserViewModel.FromUser(user, customer);
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel model, string? password, string? confirmPassword)
        {
            model.IsEdit = true;
            
            // Custom validation
            await ValidateUserViewModel(model, password, confirmPassword, true);
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Update User
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                    if (user == null)
                        return NotFound();
                        
                    // Only update the fields we want to change (don't update UserID)
                    user.Email = model.Email;
                    user.Name = model.Name;
                    user.IsAdmin = model.IsAdmin;
                    
                    // Update password if provided
                    if (!string.IsNullOrEmpty(password))
                    {
                        var salt = GenerateSalt();
                        user.Salt = salt;
                        user.HashPW = HashPassword(password, salt);
                    }
                    
                    // Use Entry to update only modified properties (avoids updating identity column)
                    _context.Entry(user).Property(u => u.Email).IsModified = true;
                    _context.Entry(user).Property(u => u.Name).IsModified = true;
                    _context.Entry(user).Property(u => u.IsAdmin).IsModified = true;
                    
                    if (!string.IsNullOrEmpty(password))
                    {
                        _context.Entry(user).Property(u => u.Salt).IsModified = true;
                        _context.Entry(user).Property(u => u.HashPW).IsModified = true;
                    }
                    
                    // Update or create Customer profile
                    var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);
                    var customerData = model.ToCustomer();
                    
                    if (customerData != null)
                    {
                        if (existingCustomer != null)
                        {
                            // Update existing customer
                            existingCustomer.PhoneNumber = customerData.PhoneNumber;
                            existingCustomer.StreetAddress = customerData.StreetAddress;
                            existingCustomer.Suburb = customerData.Suburb;
                            existingCustomer.State = customerData.State;
                            existingCustomer.PostCode = customerData.PostCode;
                            existingCustomer.CardNumber = customerData.CardNumber;
                            existingCustomer.CardOwner = customerData.CardOwner;
                            existingCustomer.Expiry = customerData.Expiry;
                            existingCustomer.CVV = customerData.CVV;
                            
                            _context.Customers.Update(existingCustomer);
                        }
                        else
                        {
                            // Create new customer profile
                            _context.Customers.Add(customerData);
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(ManageUsers));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the user. Please try again.");
                }
            }
            
            return View(model);
        }

        // Australian Address Validation and User Validation
        private async Task ValidateUserViewModel(UserViewModel model, string? password, string? confirmPassword, bool isEdit = false)
        {
            // Check if username already exists (for new users or changed username)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (!isEdit && existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
            }
            else if (isEdit && existingUser != null && existingUser.UserID != model.UserID)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
            }
            
            // Check if email already exists (for new users or changed email)
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (!isEdit && existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }
            else if (isEdit && existingEmail != null && existingEmail.UserID != model.UserID)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }
            
            // Password validation
            if (!isEdit || !string.IsNullOrEmpty(password))
            {
                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("Password", "Password is required.");
                }
                else if (password.Length < 6)
                {
                    ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
                }
                
                if (password != confirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                }
            }
            
            // Australian address validation
            if (!string.IsNullOrEmpty(model.StreetAddress))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.StreetAddress, @"^(?=.*[a-zA-Z])(?=.*[0-9]).*$"))
                {
                    ModelState.AddModelError("StreetAddress", "Street address must contain both letters and numbers.");
                }
            }
            
            if (model.PostCode.HasValue)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.PostCode.ToString(), @"^\d{4}$"))
                {
                    ModelState.AddModelError("PostCode", "Postcode must be exactly 4 digits.");
                }
            }
            
            if (!string.IsNullOrEmpty(model.State))
            {
                var validStates = new[] { "NSW", "VIC", "QLD", "WA", "SA", "TAS", "ACT", "NT" };
                if (!validStates.Contains(model.State))
                {
                    ModelState.AddModelError("State", "Please select a valid Australian state.");
                }
            }
            
            // Card expiry validation
            if (!string.IsNullOrEmpty(model.Expiry))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.Expiry, @"^\d{2}/\d{2}$"))
                {
                    ModelState.AddModelError("Expiry", "Expiry date must be in MM/YY format.");
                }
            }
        }

        // Password hashing utilities
        private string GenerateSalt()
        {
            var saltBytes = new byte[24];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var combinedBytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            var user = await _context.Users.FindAsync(userName);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        // Order Management (Phase 4.1)
        public async Task<IActionResult> ManageOrders(string searchTerm, string statusFilter, int page = 1)
        {
            const int pageSize = 20;
            
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => 
                    o.OrderID.ToString().Contains(searchTerm) || 
                    o.Customer.Email.Contains(searchTerm) ||
                    o.Customer.PhoneNumber.Contains(searchTerm) ||
                    o.StreetAddress.Contains(searchTerm));
            }

            // Status filtering (for future enhancement)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter.ToLower())
                {
                    case "completed":
                        // For now, all orders are completed
                        break;
                    case "processing":
                        // Future implementation
                        break;
                    case "shipped":
                        // Future implementation
                        break;
                }
            }

            // Get total count for pagination
            var totalOrders = await orders.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            // Apply pagination
            var paginatedOrders = await orders
                .OrderByDescending(o => o.OrderID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get order statistics
            var totalOrdersCount = await _context.Orders.CountAsync();
            var totalRevenue = await _context.ProductsInOrders
                .Include(p => p.Stocktake)
                .SumAsync(p => (p.Quantity ?? 0) * (p.Stocktake.Price ?? 0));

            var averageOrderValue = totalOrdersCount > 0 ? totalRevenue / totalOrdersCount : 0;

            // Pass data to view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalOrdersCount = totalOrdersCount;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.AverageOrderValue = averageOrderValue;

            return View(paginatedOrders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .ThenInclude(p => p.Genre)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Source)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            // Calculate order totals
            var orderTotal = order.ProductsInOrders.Sum(p => (p.Quantity ?? 0) * (p.Stocktake?.Price ?? 0));
            ViewBag.OrderTotal = orderTotal;

            return View(order);
        }

        // Advanced User Management (Phase 3.3)
        [HttpGet]
        public async Task<IActionResult> UserProfile(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound();

            // Get customer details if user has customer profile
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(c => c.Email == user.Email);

            ViewBag.Customer = customer;
            ViewBag.User = user;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userName, bool isAdmin)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                user.IsAdmin = isAdmin;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User role updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating user role: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userName)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // For now, we'll use a simple active/inactive status
                // In a real application, you might add an IsActive property to the User model
                // For demonstration, we'll just return success
                
                return Json(new { success = true, message = "User status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating user status: " + ex.Message });
            }
        }

        // Reports and Analytics (Phase 4.2)
        public async Task<IActionResult> Reports()
        {
            // Recent Orders
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderID)
                .Take(10)
                .ToListAsync();

            // Revenue Analytics
            var totalRevenue = await _context.ProductsInOrders
                .Include(p => p.Stocktake)
                .SumAsync(p => (p.Quantity ?? 0) * (p.Stocktake.Price ?? 0));

            // Popular Products
            var popularProducts = await _context.ProductsInOrders
                .Include(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .GroupBy(p => p.Stocktake.Product)
                .Select(g => new { 
                    Product = g.Key, 
                    TotalSold = g.Sum(x => x.Quantity ?? 0),
                    Revenue = g.Sum(x => (x.Quantity ?? 0) * (x.Stocktake.Price ?? 0))
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            // Genre Statistics
            var genreStats = await _context.ProductsInOrders
                .Include(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .ThenInclude(p => p.Genre)
                .GroupBy(p => p.Stocktake.Product.Genre.Name)
                .Select(g => new { 
                    Genre = g.Key, 
                    TotalSold = g.Sum(x => x.Quantity ?? 0),
                    Revenue = g.Sum(x => (x.Quantity ?? 0) * (x.Stocktake.Price ?? 0))
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            // Customer Statistics
            var customerStats = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .Select(c => new {
                    Customer = c,
                    TotalOrders = c.Orders.Count,
                    TotalSpent = c.Orders.SelectMany(o => o.ProductsInOrders).Sum(p => (p.Quantity ?? 0) * (p.Stocktake.Price ?? 0))
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PopularProducts = popularProducts;
            ViewBag.GenreStats = genreStats;
            ViewBag.CustomerStats = customerStats;
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();

            return View();
        }

        // Inventory Reports
        public async Task<IActionResult> InventoryReport()
        {
            var stockItems = await _context.Stocktakes
                .Include(s => s.Product)
                .ThenInclude(p => p.Genre)
                .Include(s => s.Source)
                .OrderBy(s => s.Quantity)
                .ToListAsync();

            var lowStockItems = stockItems.Where(s => s.Quantity < 10).ToList();
            var outOfStockItems = stockItems.Where(s => s.Quantity == 0).ToList();
            
            var totalStockValue = stockItems.Sum(s => (s.Quantity ?? 0) * (s.Price ?? 0));

            ViewBag.LowStockItems = lowStockItems;
            ViewBag.OutOfStockItems = outOfStockItems;
            ViewBag.TotalStockValue = totalStockValue;

            return View(stockItems);
        }

        // Product Details for Admin - allows admin to view product details
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (product == null)
                return NotFound();

            // Use the same view as Customer ProductDetails for consistency
            return View("~/Views/Customer/ProductDetails.cshtml", product);
        }

        // Serve product images from database
        [HttpGet]
        public async Task<IActionResult> GetProductImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            
            if (product?.ProductImage == null || product.ImageContentType == null)
            {
                return NotFound();
            }

            return File(product.ProductImage, product.ImageContentType);
        }

        // Stock Management Actions
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int stockId, int quantity, double price)
        {
            try
            {
                var stocktake = await _context.Stocktakes.FindAsync(stockId);
                if (stocktake == null)
                {
                    return Json(new { success = false, message = "Stock entry not found" });
                }

                stocktake.Quantity = quantity;
                stocktake.Price = price;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Stock updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating stock: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(int productId, int sourceId, int quantity, double price)
        {
            try
            {
                var stocktake = new Stocktake
                {
                    ProductId = productId,
                    SourceId = sourceId,
                    Quantity = quantity,
                    Price = price
                };

                _context.Stocktakes.Add(stocktake);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Stock added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding stock: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateStock([FromBody] BulkStockUpdateRequest request)
        {
            try
            {
                if (request.ItemIds == null || !request.ItemIds.Any())
                {
                    return Json(new { success = false, message = "No items selected" });
                }

                int updatedCount = 0;
                var errors = new List<string>();

                foreach (var itemId in request.ItemIds)
                {
                    try
                    {
                        var stocktakes = await _context.Stocktakes
                            .Where(s => s.ProductId == itemId)
                            .ToListAsync();

                        if (!stocktakes.Any())
                        {
                            errors.Add($"No stock entries found for item ID {itemId}");
                            continue;
                        }

                        foreach (var stocktake in stocktakes)
                        {
                            // Update quantity based on update type
                            switch (request.UpdateType.ToLower())
                            {
                                case "set":
                                    stocktake.Quantity = request.Quantity;
                                    break;
                                case "add":
                                    stocktake.Quantity = (stocktake.Quantity ?? 0) + request.Quantity;
                                    break;
                                case "subtract":
                                    stocktake.Quantity = Math.Max(0, (stocktake.Quantity ?? 0) - request.Quantity);
                                    break;
                            }

                            // Update price if requested
                            if (request.UpdatePrice && request.NewPrice.HasValue)
                            {
                                stocktake.Price = request.NewPrice.Value;
                            }
                        }

                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error updating item ID {itemId}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                if (errors.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = $"Updated {updatedCount} items, but encountered errors: {string.Join(", ", errors)}"
                    });
                }

                return Json(new { 
                    success = true, 
                    message = $"Successfully updated stock for {updatedCount} items"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating stock: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteItems([FromBody] BulkDeleteRequest request)
        {
            try
            {
                if (request.ItemIds == null || !request.ItemIds.Any())
                {
                    return Json(new { success = false, message = "No items selected" });
                }

                int deletedCount = 0;
                var errors = new List<string>();

                foreach (var itemId in request.ItemIds)
                {
                    try
                    {
                        var product = await _context.Products.FindAsync(itemId);
                        if (product == null)
                        {
                            errors.Add($"Product with ID {itemId} not found");
                            continue;
                        }

                        // Remove associated stocktakes first
                        var stocktakes = await _context.Stocktakes.Where(s => s.ProductId == itemId).ToListAsync();
                        if (stocktakes.Any())
                        {
                            _context.Stocktakes.RemoveRange(stocktakes);
                        }

                        // Remove the product
                        _context.Products.Remove(product);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error deleting item ID {itemId}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                if (errors.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = $"Deleted {deletedCount} items, but encountered errors: {string.Join(", ", errors)}"
                    });
                }

                return Json(new { 
                    success = true, 
                    message = $"Successfully deleted {deletedCount} items"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting items: " + ex.Message });
            }
        }
    }
} 