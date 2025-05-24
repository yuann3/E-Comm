using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Comm.Models;

namespace E_Comm.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EntertainmentGuildContext _context;

    public HomeController(ILogger<HomeController> logger, EntertainmentGuildContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Main Entry Point for 3B1G
    public async Task<IActionResult> Index()
    {
        // If user is already authenticated, redirect to appropriate dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            else if (User.IsInRole("Employee"))
                return RedirectToAction("Index", "Employee");
            else if (User.IsInRole("Customer"))
                return RedirectToAction("Index", "Customer");
        }

        // Show featured products for non-authenticated users
        ViewBag.FeaturedProducts = await _context.Products
            .Include(p => p.Genre)
            .Include(p => p.Stocktakes)
            .Where(p => p.Stocktakes.Any(s => s.Quantity > 0))
            .Take(6)
            .ToListAsync();

        ViewBag.Genres = await _context.Genres.ToListAsync();

        return View();
    }

    // Public Product Catalog (accessible without login)
    public async Task<IActionResult> Catalog(string searchTerm, int? genreId, int page = 1)
    {
        const int pageSize = 12;
        
        var products = _context.Products
            .Include(p => p.Genre)
            .Include(p => p.Stocktakes)
            .Where(p => p.Stocktakes.Any(s => s.Quantity > 0))
            .AsQueryable();

        // Search functionality (Business Scenario 1: Customer Searching for an Item)
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

    // Public Product Details
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

    // About 3B1G
    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}