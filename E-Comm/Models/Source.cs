using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    public class Source
    {
        [Key]
        public int SourceId { get; set; }
        
        public string? Source_name { get; set; }
        
        public string? ExternalLink { get; set; }
        
        [ForeignKey("Genre")]
        public int? GenreID { get; set; }
        
        // Navigation properties
        public virtual Genre? Genre { get; set; }
        public virtual ICollection<Stocktake> Stocktakes { get; set; } = new List<Stocktake>();
    }
} 