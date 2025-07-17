using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace E_Comm.Models
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EntertainmentGuildContext _context;
        private const string CartSessionKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor, EntertainmentGuildContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new Cart();

            var cartJson = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new Cart();

            return JsonSerializer.Deserialize<Cart>(cartJson) ?? new Cart();
        }

        public void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var cartJson = JsonSerializer.Serialize(cart);
            session.SetString(CartSessionKey, cartJson);
        }

        public async Task<bool> AddToCartAsync(int productId, int quantity = 1)
        {
            try
            {
                // Get the stocktake for this product (we'll use the first available one)
                var stocktake = await _context.Stocktakes
                    .Include(s => s.Product)
                    .ThenInclude(p => p.Genre)
                    .Include(s => s.Source)
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.Quantity > 0);

                if (stocktake == null || stocktake.Product == null)
                    return false;

                var cart = GetCart();
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    StocktakeId = stocktake.ItemId,
                    ProductName = stocktake.Product.Name,
                    ProductAuthor = stocktake.Product.Author ?? "Unknown",
                    GenreName = stocktake.Product.Genre?.Name ?? "Unknown",
                    Price = stocktake.Price ?? 0,
                    Quantity = quantity,
                    MaxQuantity = stocktake.Quantity ?? 0,
                    SourceName = stocktake.Source?.Source_name ?? "Unknown"
                };

                cart.AddItem(cartItem);
                SaveCart(cart);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RemoveFromCart(int stocktakeId)
        {
            var cart = GetCart();
            cart.RemoveItem(stocktakeId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int stocktakeId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(stocktakeId, quantity);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var cart = new Cart();
            SaveCart(cart);
        }

        public int GetCartItemCount()
        {
            return GetCart().TotalItems;
        }

        public double GetCartTotal()
        {
            return GetCart().TotalPrice;
        }

        public async Task<Order?> CreateOrderAsync(Customer customer)
        {
            var cart = GetCart();
            if (!cart.Items.Any())
                return null;

            try
            {
                var order = new Order
                {
                    CustomerId = customer.CustomerID,
                    StreetAddress = customer.StreetAddress,
                    PostCode = customer.PostCode,
                    Suburb = customer.Suburb,
                    State = customer.State,
                    Customer = customer
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items
                foreach (var item in cart.Items)
                {
                    var orderItem = new ProductsInOrder
                    {
                        OrderId = order.OrderID,
                        ProduktId = item.StocktakeId,
                        Quantity = item.Quantity
                    };

                    _context.ProductsInOrders.Add(orderItem);

                    // Update stock
                    var stocktake = await _context.Stocktakes.FindAsync(item.StocktakeId);
                    if (stocktake != null)
                    {
                        stocktake.Quantity = Math.Max(0, (stocktake.Quantity ?? 0) - item.Quantity);
                    }
                }

                await _context.SaveChangesAsync();
                ClearCart();
                return order;
            }
            catch
            {
                return null;
            }
        }
    }
} 