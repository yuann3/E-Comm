using System.ComponentModel.DataAnnotations;

namespace E_Comm.Models
{
    public class Genre
    {
        [Key]
        public int GenreID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Source> Sources { get; set; } = new List<Source>();
    }
} 