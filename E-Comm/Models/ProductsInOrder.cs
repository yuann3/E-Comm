using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class ProductsInOrder
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Order")]
        public int? OrderId { get; set; }
        
        [ForeignKey("Stocktake")]
        public int? ProduktId { get; set; } // Note: keeping original typo from database
        
        public int? Quantity { get; set; }
        
        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Stocktake? Stocktake { get; set; }
    }
} 