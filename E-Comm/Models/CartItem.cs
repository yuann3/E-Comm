using System.ComponentModel.DataAnnotations;

namespace E_Comm.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int StocktakeId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductAuthor { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } // Available stock
        public string SourceName { get; set; } = string.Empty;
        
        public double TotalPrice => Price * Quantity;
    }
    
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public int TotalItems => Items.Sum(i => i.Quantity);
        public double TotalPrice => Items.Sum(i => i.TotalPrice);
        
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.StocktakeId == item.StocktakeId);
            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(existingItem.Quantity + item.Quantity, existingItem.MaxQuantity);
            }
            else
            {
                Items.Add(item);
            }
        }
        
        public void RemoveItem(int stocktakeId)
        {
            Items.RemoveAll(i => i.StocktakeId == stocktakeId);
        }
        
        public void UpdateQuantity(int stocktakeId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.StocktakeId == stocktakeId);
            if (item != null)
            {
                item.Quantity = Math.Max(1, Math.Min(quantity, item.MaxQuantity));
            }
        }
        
        public void Clear()
        {
            Items.Clear();
        }
    }
} 