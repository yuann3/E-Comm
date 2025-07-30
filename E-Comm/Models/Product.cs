using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class Product
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Author { get; set; }
        
        public string? Description { get; set; }
        
        [ForeignKey("Genre")]
        public int? GenreID { get; set; }
        
        public int? SubGenre { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? Published { get; set; }
        
        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
        
        public DateTime? LastUpdated { get; set; }
        
        // Image fields for storing product images in database
        public byte[]? ProductImage { get; set; }
        
        [StringLength(100)]
        public string? ImageContentType { get; set; }
        
        // Navigation properties
        public virtual Genre? Genre { get; set; }
        public virtual User? LastUpdatedByUser { get; set; }
        public virtual ICollection<Stocktake> Stocktakes { get; set; } = new List<Stocktake>();
    }
} 