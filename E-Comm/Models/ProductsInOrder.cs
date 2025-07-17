using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class ProductsInOrder
    {
        [ForeignKey("Order")]
        [Column("OrderId")]
        public int OrderId { get; set; }
        
        [ForeignKey("Stocktake")]
        [Column("produktId")]
        public int ProduktId { get; set; } // Note: keeping original typo from database
        
        [Column("Quantity")]
        public int? Quantity { get; set; }
        
        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Stocktake? Stocktake { get; set; }
    }
} 