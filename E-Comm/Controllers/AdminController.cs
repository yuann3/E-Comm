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
            ViewBag.LowStockItems = await _context.Stocktakes.Where(s => s.Quantity < 10).CountAsync();

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

            return View();
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
                        products = products.Where(p => p.Stocktakes.Sum(s => s.Quantity) < 10);
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

            // Pass data to view
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.GenreFilter = genreFilter;
            ViewBag.StockFilter = stockFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;

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
        public async Task<IActionResult> CreateItem(Product product, int sourceId, int quantity, double price)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> EditItem(Product product)
        {
            if (ModelState.IsValid)
            {
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
                    // Note: Employee role only exists as test credentials, not in database
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
            var employeeCount = 0; // Employees only exist as test credentials, not in database
            var customerCount = await _context.Users.CountAsync(u => !u.IsAdmin);

            // Pass data to view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalUsersCount = totalUsersCount;
            ViewBag.AdminCount = adminCount;
            ViewBag.EmployeeCount = employeeCount;
            ViewBag.CustomerCount = customerCount;

            return View(paginatedUsers);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                // In a real implementation, password would be hashed
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageUsers));
            }
            return View(user);
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

        // Reports and Analytics
        public async Task<IActionResult> Reports()
        {
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderID)
                .Take(10)
                .ToListAsync();

            return View(recentOrders);
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
                        message = $"Updated {updatedCount} items with errors: {string.Join(", ", errors)}" 
                    });
                }

                return Json(new { 
                    success = true, 
                    message = $"Stock updated successfully for {updatedCount} items",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating stock: " + ex.Message });
            }
        }
    }
} 