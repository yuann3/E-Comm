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

        // Customer Dashboard - Entry Point for Customers.
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Customer";

            // Load customer and their recent orders (optional for Home view)
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

            // Render Home/Index.cshtml directly to avoid redirect loops
            return View("~/Views/Home/Index.cshtml");
        }

        // Browse Products
        public async Task<IActionResult> Browse(string searchTerm, int? genreId, int page = 1)
        {
            const int pageSize = 12;
            var products = _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .Where(p => p.Stocktakes.Any(s => s.Quantity > 0));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Author.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }
            if (genreId.HasValue)
                products = products.Where(p => p.GenreID == genreId);

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

        // Remaining actions unchanged...
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .FirstOrDefaultAsync(p => p.ID == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            TempData["Success"] = "Item added to cart successfully!";
            return RedirectToAction("ProductDetails", new { id = productId });
        }

        public async Task<IActionResult> MyAccount()
        {
            var customerEmail = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == customerEmail);
            if (customer == null) return RedirectToAction("CreateAccount");
            return View(customer);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateAccount() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);
            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Email == customer.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(customer);
            }
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Account created successfully! You can now login.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return RedirectToAction("CreateAccount");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);
            var email = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (existing != null)
            {
                existing.PhoneNumber = customer.PhoneNumber;
                existing.StreetAddress = customer.StreetAddress;
                existing.PostCode = customer.PostCode;
                existing.Suburb = customer.Suburb;
                existing.State = customer.State;
                existing.CardNumber = customer.CardNumber;
                existing.CardOwner = customer.CardOwner;
                existing.Expiry = customer.Expiry;
                existing.CVV = customer.CVV;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Account details updated successfully!";
                return RedirectToAction("MyAccount");
            }
            return View(customer);
        }

        public async Task<IActionResult> OrderHistory()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return RedirectToAction("CreateAccount");
            return View(customer.Orders.OrderByDescending(o => o.OrderDate));
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.Customer.Email == email);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Checkout()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return RedirectToAction("CreateAccount");
            ViewBag.Customer = customer;
            return View();
        }

        public IActionResult ContactSupport() => View();
    }
}
