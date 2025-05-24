using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class Stocktake
    {
        [Key]
        public int ItemId { get; set; }
        
        [ForeignKey("Source")]
        public int? SourceId { get; set; }
        
        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        
        public int? Quantity { get; set; }
        
        public double? Price { get; set; }
        
        // Navigation properties
        public virtual Source? Source { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ICollection<ProductsInOrder> ProductsInOrders { get; set; } = new List<ProductsInOrder>();
    }
} 