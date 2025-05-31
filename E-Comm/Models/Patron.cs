using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    [Table("Patrons")]
    public class Patron
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [Column("Email")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Column("Name")]
        [StringLength(255)]
        public string? Name { get; set; }

        [Column("Salt")]
        [StringLength(32)]
        public string? Salt { get; set; }

        [Column("HashPW")]
        [StringLength(64)]
        public string? HashPW { get; set; }

        // Navigation property
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
} 