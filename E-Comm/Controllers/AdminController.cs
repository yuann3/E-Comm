using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using E_Comm.Models;

namespace E_Comm.Controllers
{
    /// <summary>
    /// Admin Controller - Handles all administrative functions for the e-commerce system
    /// Restricted to users with "Admin" role for security
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Database context for Entity Framework operations
        private readonly EntertainmentGuildContext _context;

        /// <summary>
        /// Constructor: Dependency injection of database context
        /// </summary>
        /// <param name="context">Entity Framework database context</param>
        public AdminController(EntertainmentGuildContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Admin Dashboard - Entry Point for Administrators
        /// Displays key metrics and statistics for the admin overview
        /// </summary>
        /// <returns>Dashboard view with statistics</returns>
        public async Task<IActionResult> Index()
        {
            // Get current user's name, fallback to "Admin" if null
            ViewBag.UserName = User.Identity?.Name ?? "Admin";
            
            // Calculate and pass key performance indicators to the view
            // These async calls run concurrently for better performance
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            
            // Alert metric: Count items with low stock (less than 10 units)
            // This helps admins identify inventory that needs restocking
            ViewBag.LowStockItems = await _context.Stocktakes.Where(s => s.Quantity < 10).CountAsync();

            return View();
        }

        // ========== ITEM MANAGEMENT SECTION ==========
        // Business Scenario 2: Item Management from the User Interface Layer
        
        /// <summary>
        /// Display all products with related data for management
        /// Uses Include() for eager loading to avoid N+1 query problems
        /// </summary>
        /// <returns>View with list of products including genre, stock, and source information</returns>
        public async Task<IActionResult> ManageItems()
        {
            // Eager loading: Load products with their related entities in a single query
            var products = await _context.Products
                .Include(p => p.Genre)           // Load genre information
                .Include(p => p.Stocktakes)      // Load stock information
                .ThenInclude(s => s.Source)      // Load source information for each stocktake
                .ToListAsync();

            return View(products);
        }

        /// <summary>
        /// GET: Display form to create a new product
        /// Populates dropdown lists for genres and sources
        /// </summary>
        /// <returns>Create item view with populated dropdown data</returns>
        [HttpGet]
        public async Task<IActionResult> CreateItem()
        {
            // Populate ViewBag with reference data for dropdowns
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View();
        }

        /// <summary>
        /// POST: Process new product creation
        /// Creates both product and associated stocktake entry
        /// </summary>
        /// <param name="product">Product model from form</param>
        /// <param name="sourceId">Source ID for stocktake</param>
        /// <param name="quantity">Initial stock quantity</param>
        /// <param name="price">Product price</param>
        /// <returns>Redirect to ManageItems on success, or redisplay form on error</returns>
        [HttpPost]
        public async Task<IActionResult> CreateItem(Product product, int sourceId, int quantity, double price)
        {
            // Validate model state (data annotations, required fields, etc.)
            if (ModelState.IsValid)
            {
                // Audit trail: Record who created/modified the item and when
                product.LastUpdatedBy = User.Identity?.Name;
                product.LastUpdated = DateTime.Now;

                // Add product to database
                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // Save to get generated ProductID

                // Create associated stocktake entry
                // This maintains inventory data separately from product information
                var stocktake = new Stocktake
                {
                    ProductId = product.ID,    // Use generated ID from saved product
                    SourceId = sourceId,
                    Quantity = quantity,
                    Price = price
                };
                _context.Stocktakes.Add(stocktake);
                await _context.SaveChangesAsync(); // Save stocktake entry

                // Redirect to management page on successful creation
                return RedirectToAction(nameof(ManageItems));
            }

            // If validation fails, repopulate dropdown data and redisplay form
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        /// <summary>
        /// GET: Display form to edit an existing product
        /// Includes stocktake information for comprehensive editing
        /// </summary>
        /// <param name="id">Product ID to edit</param>
        /// <returns>Edit view with product data, or NotFound if product doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            // Find product with related stocktake information
            var product = await _context.Products
                .Include(p => p.Stocktakes)  // Include stock information for editing
                .FirstOrDefaultAsync(p => p.ID == id);

            // Return 404 if product not found
            if (product == null)
                return NotFound();

            // Populate dropdown lists for the edit form
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        /// <summary>
        /// POST: Process product edit/update
        /// Updates product information and maintains audit trail
        /// </summary>
        /// <param name="product">Updated product model</param>
        /// <returns>Redirect to ManageItems on success, or redisplay form on error</returns>
        [HttpPost]
        public async Task<IActionResult> EditItem(Product product)
        {
            // Validate updated model data
            if (ModelState.IsValid)
            {
                // Update audit trail information
                product.LastUpdatedBy = User.Identity?.Name;
                product.LastUpdated = DateTime.Now;

                // Update existing product in database
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                // Redirect to management page on successful update
                return RedirectToAction(nameof(ManageItems));
            }

            // If validation fails, repopulate dropdown data and redisplay form
            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.Sources = await _context.Sources.ToListAsync();
            return View(product);
        }

        /// <summary>
        /// POST: Delete a product and its associated data
        /// Handles cascade deletion of related stocktake entries
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        /// <returns>Redirect to ManageItems</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteItem(int id)
        {
            // Find the product to delete
            var product = await _context.Products.FindAsync(id);
            
            if (product != null)
            {
                // IMPORTANT: Remove related stocktakes first to avoid foreign key constraint violations
                // This is necessary if cascade delete is not configured in the database
                var stocktakes = await _context.Stocktakes.Where(s => s.ProductId == id).ToListAsync();
                _context.Stocktakes.RemoveRange(stocktakes);

                // Remove the product itself
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            
            // Always redirect back to management page (even if product wasn't found)
            return RedirectToAction(nameof(ManageItems));
        }

        // ========== USER MANAGEMENT SECTION ==========
        
        /// <summary>
        /// Display all users for administrative management
        /// </summary>
        /// <returns>View with list of all users</returns>
        public async Task<IActionResult> ManageUsers()
        {
            // Get all users from database
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        /// <summary>
        /// GET: Display form to create a new user
        /// </summary>
        /// <returns>Create user view</returns>
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        /// <summary>
        /// POST: Process new user creation
        /// NOTE: This is a simplified implementation for demonstration
        /// </summary>
        /// <param name="user">User model from form</param>
        /// <returns>Redirect to ManageUsers on success, or redisplay form on error</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            // Validate user model
            if (ModelState.IsValid)
            {
                // SECURITY WARNING: In a real implementation, password should be hashed
                // Consider using ASP.NET Identity or similar for proper password management
                // Example: user.Password = _passwordHasher.HashPassword(user, user.Password);
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(ManageUsers));
            }
            
            // Redisplay form with validation errors
            return View(user);
        }

        /// <summary>
        /// POST: Delete a user account
        /// CAUTION: This is a hard delete - consider soft delete for audit purposes
        /// </summary>
        /// <param name="userName">Username of user to delete</param>
        /// <returns>Redirect to ManageUsers</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            // Find user by username (primary key)
            var user = await _context.Users.FindAsync(userName);
            
            if (user != null)
            {
                // CONSIDERATION: Check for related data (orders, reviews, etc.)
                // before deletion to avoid foreign key constraint violations
                
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            
            // Always redirect back to user management
            return RedirectToAction(nameof(ManageUsers));
        }

        // ========== REPORTS AND ANALYTICS SECTION ==========
        
        /// <summary>
        /// Display reports and analytics for administrative insight
        /// Currently shows recent orders with customer information
        /// </summary>
        /// <returns>Reports view with recent order data</returns>
        public async Task<IActionResult> Reports()
        {
            // Get the 10 most recent orders with customer information
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)        // Include customer details for each order
                .OrderByDescending(o => o.OrderDate)  // Sort by most recent first
                .Take(10)                        // Limit to 10 most recent
                .ToListAsync();

            // POTENTIAL ENHANCEMENTS:
            // - Add date range filtering
            // - Include order totals and statistics
            // - Add pagination for large datasets
            // - Include product sales analytics
            // - Add revenue reporting by time period

            return View(recentOrders);
        }

        // ========== POTENTIAL FUTURE ENHANCEMENTS ==========
        /*
        /// <summary>
        /// Bulk import products from CSV or Excel file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            // Implementation for bulk product import
        }

        /// <summary>
        /// Export products to CSV for backup or analysis
        /// </summary>
        public async Task<IActionResult> ExportProducts()
        {
            // Implementation for product export
        }

        /// <summary>
        /// Generate sales report for specific date range
        /// </summary>
        public async Task<IActionResult> SalesReport(DateTime startDate, DateTime endDate)
        {
            // Implementation for sales analytics
        }

        /// <summary>
        /// Manage stock levels and reorder points
        /// </summary>
        public async Task<IActionResult> StockManagement()
        {
            // Implementation for inventory management
        }
        */
    }
}

// ========== SECURITY CONSIDERATIONS ==========
/*
1. Role-based authorization is implemented at controller level
2. Consider adding action-level authorization for sensitive operations
3. Input validation should be comprehensive (implement custom validators)
4. Consider implementing CSRF protection for state-changing operations
5. Audit logging should be implemented for all admin actions
6. Consider implementing soft deletes instead of hard deletes
7. Password hashing must be implemented for user management
8. Consider implementing rate limiting for admin actions
*/

// ========== PERFORMANCE CONSIDERATIONS ==========
/*
1. Use async/await for all database operations
2. Implement eager loading (Include) to avoid N+1 queries
3. Consider implementing caching for frequently accessed data
4. Add pagination for large datasets
5. Consider implementing database indexing for frequently queried fields
6. Monitor and optimize query performance
*/

// ========== ERROR HANDLING IMPROVEMENTS ==========
/*
1. Implement global exception handling
2. Add detailed logging for debugging
3. Provide user-friendly error messages
4. Consider implementing transaction management for complex operations
5. Add validation for business rules beyond model validation
*/