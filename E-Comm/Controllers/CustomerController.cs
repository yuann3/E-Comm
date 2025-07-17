using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using E_Comm.Data;
using E_Comm.Models;
using E_Comm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace E_Comm.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class CustomerController : Controller
    {
        private readonly EntertainmentGuildContext _context;
        private readonly ILogger<CustomerController> _logger;
        private readonly int _pageSize;
        private readonly int _featuredCount;

        public CustomerController(
            EntertainmentGuildContext context,
            IConfiguration config,
            ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
            _pageSize = config.GetValue<int>("Pagination:PageSize", 12);
            _featuredCount = config.GetValue<int>("Pagination:FeaturedCount", 6);
        }

        // GET: /Customer
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Customer";
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;

<<<<<<< HEAD
            // Load customer and their recent orders (optional for Home view)
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
              //  .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Email == customerEmail);
=======
                // Fetch latest 5 orders
                var customer = await _context.Customers
                    .Include(c => c.Orders.OrderByDescending(o => o.OrderDate).Take(5))
                    .FirstOrDefaultAsync(c => c.Email == email);
>>>>>>> upstream/main

                ViewBag.RecentOrders = customer?.Orders.ToList() ?? new List<Order>();

                // Featured products
                var featured = await _context.Products
                    .Include(p => p.Genre)
                    .Include(p => p.Stocktakes)
                    .Where(p => p.Stocktakes.Any(s => s.Quantity > 0))
                    .OrderBy(p => p.Name)
                    .Take(_featuredCount)
                    .Select(p => new ProductCardViewModel
                    {
                        Id = p.ID,
                        Name = p.Name,
                        Genre = p.Genre.Name,
                        Price = p.Price,
                        Stock = p.Stocktakes.Sum(s => s.Quantity),
                        ThumbnailUrl = p.ThumbnailUrl
                    })
                    .ToListAsync();

                ViewBag.FeaturedProducts = featured;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer dashboard.");
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Customer/Browse
        public async Task<IActionResult> Browse(string searchTerm, int? genreId, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .Where(p => p.Stocktakes.Any(s => s.Quantity > 0));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Author.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }
            if (genreId.HasValue)
                query = query.Where(p => p.GenreID == genreId.Value);

            var total = await query.CountAsync();
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .Select(p => new ProductCardViewModel
                {
                    Id = p.ID,
                    Name = p.Name,
                    Genre = p.Genre.Name,
                    Price = p.Price,
                    Stock = p.Stocktakes.Sum(s => s.Quantity),
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();

            var vm = new BrowseProductsViewModel
            {
                Products = products,
                Genres = await _context.Genres.ToListAsync(),
                SearchTerm = searchTerm,
                SelectedGenre = genreId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)_pageSize)
            };

            return View(vm);
        }

        // GET: /Customer/ProductDetails/5
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

        // POST: /Customer/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // TODO: integrate a CartService (session, cookies, or database)
            TempData["Success"] = "Item added to cart successfully!";
            return RedirectToAction(nameof(ProductDetails), new { id = productId });
        }

        // GET: /Customer/MyAccount
        public async Task<IActionResult> MyAccount()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return RedirectToAction(nameof(CreateAccount));

            var vm = new CustomerEditViewModel
            {
                PhoneNumber = customer.PhoneNumber,
                StreetAddress = customer.StreetAddress,
                Suburb = customer.Suburb,
                State = customer.State,
                PostCode = customer.PostCode,
                PaymentToken = customer.PaymentToken
            };

            return View(vm);
        }

        // GET: /Customer/CreateAccount
        [AllowAnonymous]
        public IActionResult CreateAccount() => View();

        // POST: /Customer/CreateAccount
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CustomerRegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (await _context.Customers.AnyAsync(c => c.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(vm);
            }

            var customer = new Customer
            {
                Email = vm.Email,
                PasswordHash = HashPassword(vm.Password), // utilize a secure hasher
                PhoneNumber = vm.PhoneNumber,
                StreetAddress = vm.StreetAddress,
                Suburb = vm.Suburb,
                State = vm.State,
                PostCode = vm.PostCode,
                PaymentToken = vm.PaymentToken
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully! You can now login.";
            return RedirectToAction("Login", "Auth");
        }

        // POST: /Customer/EditAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(CustomerEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return RedirectToAction(nameof(CreateAccount));

            // Map safely
            customer.PhoneNumber = vm.PhoneNumber;
            customer.StreetAddress = vm.StreetAddress;
            customer.Suburb = vm.Suburb;
            customer.State = vm.State;
            customer.PostCode = vm.PostCode;
            customer.PaymentToken = vm.PaymentToken;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Account details updated successfully!";
            return RedirectToAction(nameof(MyAccount));
        }

        // GET: /Customer/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var orders = await _context.Orders
                .Include(o => o.ProductsInOrders)
                    .ThenInclude(p => p.Stocktake)
                        .ThenInclude(s => s.Product)
                .Where(o => o.Customer.Email == email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Customer/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                    .ThenInclude(p => p.Stocktake)
                        .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.Customer.Email == email);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: /Customer/Checkout
        public async Task<IActionResult> Checkout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return RedirectToAction(nameof(CreateAccount));

            return View(customer);
        }

        // GET: /Customer/ContactSupport
        public IActionResult ContactSupport() => View();

        // Utilities
        private string HashPassword(string password)
        {
            // TODO: inject a proper IPasswordHasher<T>
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
