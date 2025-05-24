using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using E_Comm.Models;

namespace E_Comm.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly EntertainmentGuildContext _context;

        public CustomerController(EntertainmentGuildContext context)
        {
            _context = context;
        }

        // Customer Dashboard - Entry Point for Customers
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Customer";
            
            // Get customer's recent orders if any
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
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

        // Add to Cart (simplified - in real implementation would use session/cookies)
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // This would typically add to session cart
            // For now, just redirect back with success message
            TempData["Success"] = "Item added to cart successfully!";
            return RedirectToAction("ProductDetails", new { id = productId });
        }

        // My Account Management (Business Scenario 4: Manage a User Account)
        public async Task<IActionResult> MyAccount()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
            {
                // Redirect to create account if customer doesn't exist
                return RedirectToAction("CreateAccount");
            }

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
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
                return RedirectToAction("CreateAccount");

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
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
                    TempData["Success"] = "Account details updated successfully!";
                    return RedirectToAction("MyAccount");
                }
            }

            return View(customer);
        }

        // Order History (Customers can track their order history)
        public async Task<IActionResult> OrderHistory()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
                return RedirectToAction("CreateAccount");

            return View(customer.Orders.OrderByDescending(o => o.OrderDate));
        }

        // Order Details
        public async Task<IActionResult> OrderDetails(int id)
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.Customer.Email == customerEmail);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Checkout (simplified)
        public async Task<IActionResult> Checkout()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerEmail);

            if (customer == null)
                return RedirectToAction("CreateAccount");

            // In a real implementation, this would process cart items
            ViewBag.Customer = customer;
            return View();
        }

        // Contact Support
        public IActionResult ContactSupport()
        {
            return View();
        }
    }
} 