using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using E_Comm.Models;

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
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.LowStockItems = await _context.Stocktakes.Where(s => s.Quantity < 10).CountAsync();

            return View();
        }

        // Item Management (Business Scenario 2: Item Management from the User Interface Layer)
        public async Task<IActionResult> ManageItems()
        {
            var products = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .ToListAsync();

            return View(products);
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
                product.LastUpdatedBy = User.Identity?.Name;
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
                product.LastUpdatedBy = User.Identity?.Name;
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

        // User Management
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
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
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            return View(recentOrders);
        }
    }
} 