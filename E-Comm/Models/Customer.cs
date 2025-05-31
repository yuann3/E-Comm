using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    [Table("TO")]
    public class Customer
    {
        [Key]
        [Column("customerID")]
        public int CustomerID { get; set; }
        
        [Column("PatronId")]
        public int? PatronId { get; set; }
        
        [Required]
        [StringLength(255)]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Column("PhoneNumber")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(255)]
        [Column("StreetAddress")]
        public string? StreetAddress { get; set; }
        
        [Column("PostCode")]
        public int? PostCode { get; set; }
        
        [StringLength(50)]
        [Column("Suburb")]
        public string? Suburb { get; set; }
        
        [StringLength(50)]
        [Column("State")]
        public string? State { get; set; }
        
        [StringLength(50)]
        [Column("CardNumber")]
        public string? CardNumber { get; set; }
        
        [StringLength(50)]
        [Column("CardOwner")]
        public string? CardOwner { get; set; }
        
        [StringLength(5)]
        [Column("Expiry")]
        public string? Expiry { get; set; }
        
        [Column("CVV")]
        public int? CVV { get; set; }
        
        // Navigation properties
        public virtual Patron? Patron { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
} 