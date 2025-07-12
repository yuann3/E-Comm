using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using E_Comm.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace E_Comm.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly EntertainmentGuildContext _context;
        private readonly CartService _cartService;

        public CustomerController(EntertainmentGuildContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // Customer Dashboard - Entry Point for Customers.
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Customer";
            
            // Get customer's recent orders if any
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            ViewBag.RecentOrders = customer?.Orders?.Take(5).ToList() ?? new List<Order>();
            ViewBag.FeaturedProducts = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .Where(p => p.Stocktakes.Any(s => s.Quantity > 0))
                .Take(6)
                .ToListAsync();

            return View();
        }

        // Browse Products (Business Scenario 1: Customer Searching for an Item)
        public async Task<IActionResult> Browse(string searchTerm, int? genreId, int page = 1)
        {
            const int pageSize = 12;
            
            var products = _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .Where(p => p.Stocktakes.Any(s => s.Quantity > 0))
                .AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => 
                    p.Name.Contains(searchTerm) || 
                    p.Author.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            if (genreId.HasValue)
            {
                products = products.Where(p => p.GenreID == genreId);
            }

            var totalProducts = await products.CountAsync();
            var paginatedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedGenre = genreId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return View(paginatedProducts);
        }

        // Product Details
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // Add to Cart (Business Scenario 1: Customer adds items to cart)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var success = await _cartService.AddToCartAsync(productId, quantity);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    // Return JSON for AJAX requests
                    if (success)
                    {
                        return Json(new { success = true, message = "Item added to cart successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to add item to cart." });
                    }
                }
                else
                {
                    // Handle regular form submissions - redirect back
                    if (success)
                    {
                        TempData["Success"] = "Item added to cart successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to add item to cart. Product may be out of stock.";
                    }
                    
                    // Redirect back to the previous page or product details
                    var referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }
                    else
                    {
                        return RedirectToAction("ProductDetails", new { id = productId });
                    }
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while adding item to cart." });
                }
                else
                {
                    TempData["Error"] = "An error occurred while adding item to cart.";
                    return RedirectToAction("ProductDetails", new { id = productId });
                }
            }
        }

        // View Cart
        public IActionResult ViewCart()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // Remove from Cart
        [HttpPost]
        public IActionResult RemoveFromCart(int stocktakeId)
        {
            _cartService.RemoveFromCart(stocktakeId);
            TempData["Success"] = "Item removed from cart successfully!";
            return RedirectToAction("ViewCart");
        }

        // Update Cart Quantity
        [HttpPost]
        public IActionResult UpdateCartQuantity(int stocktakeId, int quantity)
        {
            try
            {
                _cartService.UpdateQuantity(stocktakeId, quantity);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // Clear Cart
        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            TempData["Success"] = "Cart cleared successfully!";
            return RedirectToAction("ViewCart");
        }

        // Get Cart Count (for navigation)
        public IActionResult GetCartCount()
        {
            var count = _cartService.GetCartItemCount();
            return Json(new { count = count });
        }

        // My Account Management (Business Scenario 4: Manage a User Account)
        public async Task<IActionResult> MyAccount()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
            {
                // Create a new customer object with email pre-filled for first-time setup
                customer = new Customer 
                { 
                    Email = customerEmail ?? string.Empty 
                };
                ViewBag.IsNewCustomer = true;
                ViewBag.Message = "Complete your account information below to get started.";
            }
            else
            {
                ViewBag.IsNewCustomer = false;
            }

            return View(customer);
        }

        // POST: Update/Create Customer Account Information
        [HttpPost]
        public async Task<IActionResult> MyAccount(Customer customer)
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            
            // Ensure the email matches the logged-in user
            customer.Email = customerEmail ?? string.Empty;

            // Business Scenario 4: Australian Address Validation
            ValidateAustralianAddress(customer);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Email == customerEmail);

                    if (existingCustomer != null)
                    {
                        // Update existing customer - use explicit property setting for better tracking
                        existingCustomer.PhoneNumber = customer.PhoneNumber;
                        existingCustomer.StreetAddress = customer.StreetAddress;
                        existingCustomer.PostCode = customer.PostCode;
                        existingCustomer.Suburb = customer.Suburb;
                        existingCustomer.State = customer.State;
                        existingCustomer.CardNumber = customer.CardNumber;
                        existingCustomer.CardOwner = customer.CardOwner;
                        existingCustomer.Expiry = customer.Expiry;
                        existingCustomer.CVV = customer.CVV;

                        // Mark entity as modified to ensure EF tracks the changes
                        _context.Entry(existingCustomer).State = EntityState.Modified;
                        
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Your account information has been updated successfully!";
                    }
                    else if (customer.CustomerID > 0)
                    {
                        // This is an existing customer but not found by email - use the ID
                        var customerById = await _context.Customers.FindAsync(customer.CustomerID);
                        if (customerById != null)
                        {
                            customerById.PhoneNumber = customer.PhoneNumber;
                            customerById.StreetAddress = customer.StreetAddress;
                            customerById.PostCode = customer.PostCode;
                            customerById.Suburb = customer.Suburb;
                            customerById.State = customer.State;
                            customerById.CardNumber = customer.CardNumber;
                            customerById.CardOwner = customer.CardOwner;
                            customerById.Expiry = customer.Expiry;
                            customerById.CVV = customer.CVV;

                            _context.Entry(customerById).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            TempData["Success"] = "Your account information has been updated successfully!";
                        }
                    }
                    else
                    {
                        // Create new customer record
                        customer.CustomerID = 0; // Ensure it's treated as new
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Your account information has been saved successfully!";
                    }

                    return RedirectToAction("MyAccount");
                }
                catch (Exception ex)
                {
                    // Log the error and show a user-friendly message
                    TempData["Error"] = "An error occurred while saving your information. Please try again.";
                    
                    // If validation failed, determine if this is a new or existing customer for the view
                    var existingCheck = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Email == customerEmail);
                    ViewBag.IsNewCustomer = existingCheck == null;
                    
                    return View(customer);
                }
            }

            // If validation failed, determine if this is a new or existing customer for the view
            var existingCustomerForView = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);
            ViewBag.IsNewCustomer = existingCustomerForView == null;

            return View(customer);
        }

        // Create Account (Business Scenario 3: Create a User Account)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if customer with email already exists
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == customer.Email);

                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(customer);
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Account created successfully! You can now login.";
                return RedirectToAction("Login", "Auth");
            }

            return View(customer);
        }

        // Edit Account Details
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            // Redirect to MyAccount page instead - it handles both new and existing customers
            return RedirectToAction("MyAccount");
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(Customer customer)
        {
            // Business Scenario 4: Australian Address Validation
            ValidateAustralianAddress(customer);

            if (ModelState.IsValid)
            {
                var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == customerEmail);

                if (existingCustomer != null)
                {
                    // Update customer details (Business Scenario 4 - Address validation)
                    existingCustomer.PhoneNumber = customer.PhoneNumber;
                    existingCustomer.StreetAddress = customer.StreetAddress;
                    existingCustomer.PostCode = customer.PostCode;
                    existingCustomer.Suburb = customer.Suburb;
                    existingCustomer.State = customer.State;
                    existingCustomer.CardNumber = customer.CardNumber;
                    existingCustomer.CardOwner = customer.CardOwner;
                    existingCustomer.Expiry = customer.Expiry;
                    existingCustomer.CVV = customer.CVV;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Address has been updated successfully!";
                    return RedirectToAction("MyAccount");
                }
            }

            // If we got this far, something failed, redisplay form with validation errors
            return View(customer);
        }

        // Business Scenario 4: Validation method for Australian address requirements
        private void ValidateAustralianAddress(Customer customer)
        {
            // Address Line validation - must contain both letters and numbers
            if (!string.IsNullOrEmpty(customer.StreetAddress))
            {
                bool hasLetters = System.Text.RegularExpressions.Regex.IsMatch(customer.StreetAddress, @"[a-zA-Z]");
                bool hasNumbers = System.Text.RegularExpressions.Regex.IsMatch(customer.StreetAddress, @"[0-9]");
                
                if (!hasLetters || !hasNumbers)
                {
                    ModelState.AddModelError("StreetAddress", "Address must contain both letters and numbers (e.g. 377 Ring Road)");
                }
            }

            // Post Code validation - numbers only
            if (customer.PostCode.HasValue)
            {
                string postCodeStr = customer.PostCode.ToString();
                if (!System.Text.RegularExpressions.Regex.IsMatch(postCodeStr, @"^\d+$"))
                {
                    ModelState.AddModelError("PostCode", "Post code must contain numbers only (e.g. 2308 not 'two thousand and eight')");
                }
                
                // Australian post codes should be 4 digits
                if (postCodeStr.Length != 4)
                {
                    ModelState.AddModelError("PostCode", "Australian post codes must be 4 digits");
                }
            }

            // State validation - must be one of the valid Australian states
            if (!string.IsNullOrEmpty(customer.State))
            {
                var validStates = new[] { "NSW", "VIC", "QLD", "WA", "SA", "TAS", "ACT", "NT", 
                                        "New South Wales", "Victoria", "Queensland", "Western Australia", 
                                        "South Australia", "Tasmania", "Australian Capital Territory", "Northern Territory" };
                
                if (!validStates.Contains(customer.State))
                {
                    ModelState.AddModelError("State", "Please select a valid Australian state");
                }
            }

            // Card expiry validation if provided
            if (!string.IsNullOrEmpty(customer.Expiry))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(customer.Expiry, @"^\d{2}/\d{2}$"))
                {
                    ModelState.AddModelError("Expiry", "Expiry date must be in MM/YY format");
                }
            }
        }

        // Order History (Customers can track their order history)
        public async Task<IActionResult> OrderHistory()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .ThenInclude(p => p.Genre)
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
            {
                // Show empty order history for new customers
                ViewBag.Message = "Complete your account information first to start placing orders.";
                return View(new List<Order>());
            }

            return View(customer.Orders.OrderByDescending(o => o.OrderID).ToList());
        }

        // Order Details
        public async Task<IActionResult> OrderDetails(int id)
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .ThenInclude(p => p.Genre)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.Customer.Email == customerEmail);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Checkout (Business Scenario 1: Customer proceeds to checkout)
        public async Task<IActionResult> Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before proceeding to checkout.";
                return RedirectToAction("ViewCart");
            }

            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
            {
                TempData["Error"] = "Please complete your account information before proceeding to checkout.";
                return RedirectToAction("MyAccount");
            }

            // Validate customer has required address information
            if (string.IsNullOrEmpty(customer.StreetAddress) || 
                !customer.PostCode.HasValue || 
                string.IsNullOrEmpty(customer.State))
            {
                TempData["Error"] = "Please complete your address information before proceeding to checkout.";
                return RedirectToAction("MyAccount");
            }

            ViewBag.Customer = customer;
            ViewBag.Cart = cart;
            return View();
        }

        // Process Checkout
        [HttpPost]
        public async Task<IActionResult> ProcessCheckout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("ViewCart");
            }

            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
            {
                TempData["Error"] = "Customer information not found.";
                return RedirectToAction("MyAccount");
            }

            try
            {
                var order = await _cartService.CreateOrderAsync(customer);
                if (order != null)
                {
                    TempData["Success"] = $"Order #{order.OrderID} placed successfully! Thank you for your purchase.";
                    return RedirectToAction("OrderDetails", new { id = order.OrderID });
                }
                else
                {
                    TempData["Error"] = "Failed to process your order. Please try again.";
                    return RedirectToAction("Checkout");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        // Contact Support
        public IActionResult ContactSupport()
        {
            return View();
        }
    }
} 