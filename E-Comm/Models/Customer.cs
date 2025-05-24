using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        
        public int? PatronId { get; set; }
        
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(255)]
        public string? StreetAddress { get; set; }
        
        public int? PostCode { get; set; }
        
        [StringLength(50)]
        public string? Suburb { get; set; }
        
        [StringLength(50)]
        public string? State { get; set; }
        
        [StringLength(50)]
        public string? CardNumber { get; set; }
        
        [StringLength(50)]
        public string? CardOwner { get; set; }
        
        [StringLength(5)]
        public string? Expiry { get; set; }
        
        public int? CVV { get; set; }
        
        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
} 