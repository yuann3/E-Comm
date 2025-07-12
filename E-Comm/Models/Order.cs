using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        
        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        
        [StringLength(255)]
        public string? StreetAddress { get; set; }
        
        public int? PostCode { get; set; }
        
        [StringLength(255)]
        public string? Suburb { get; set; }
        
        [StringLength(50)]
        public string? State { get; set; }
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<ProductsInOrder> ProductsInOrders { get; set; } = new List<ProductsInOrder>();
    }
} 