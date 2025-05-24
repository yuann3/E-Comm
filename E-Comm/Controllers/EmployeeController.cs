using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using E_Comm.Models;

namespace E_Comm.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly EntertainmentGuildContext _context;

        public EmployeeController(EntertainmentGuildContext context)
        {
            _context = context;
        }

        // Employee Dashboard - Entry Point for Employees
        public async Task<IActionResult> Index()
        {
            ViewBag.UserName = User.Identity?.Name ?? "Employee";
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync();
            ViewBag.LowStockItems = await _context.Stocktakes.Where(s => s.Quantity < 10).CountAsync();

            return View();
        }

        // View Items (Employees can view items available in store)
        public async Task<IActionResult> ViewItems()
        {
            var products = await _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .ThenInclude(s => s.Source)
                .ToListAsync();

            return View(products);
        }

        // Search Items (Business Scenario 1: Customer Searching for an Item - from employee perspective)
        [HttpGet]
        public async Task<IActionResult> SearchItems(string searchTerm, int? genreId)
        {
            var products = _context.Products
                .Include(p => p.Genre)
                .Include(p => p.Stocktakes)
                .AsQueryable();

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

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedGenre = genreId;

            return View(await products.ToListAsync());
        }

        // View Item Details
        public async Task<IActionResult> ItemDetails(int id)
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

        // View Accounts (Employees can view accounts)
        public async Task<IActionResult> ViewAccounts()
        {
            var customers = await _context.Customers
                .Include(c => c.Orders)
                .ToListAsync();

            return View(customers);
        }

        // View Customer Details
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // View Orders
        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // View Order Details
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.ProductsInOrders)
                .ThenInclude(p => p.Stocktake)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Stock Reports
        public async Task<IActionResult> StockReport()
        {
            var stockItems = await _context.Stocktakes
                .Include(s => s.Product)
                .Include(s => s.Source)
                .OrderBy(s => s.Quantity)
                .ToListAsync();

            return View(stockItems);
        }

        // Low Stock Alert
        public async Task<IActionResult> LowStockAlert()
        {
            var lowStockItems = await _context.Stocktakes
                .Include(s => s.Product)
                .Include(s => s.Source)
                .Where(s => s.Quantity < 10)
                .OrderBy(s => s.Quantity)
                .ToListAsync();

            return View(lowStockItems);
        }
    }
} 