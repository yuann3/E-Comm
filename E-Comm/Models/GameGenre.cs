using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Comm.Models
{
    [Table("Game_genre")]
    public class GameGenre
    {
        [Key]
        [Column("subGenreID")]
        public int SubGenreID { get; set; }

        [Column("Name")]
        [StringLength(50)]
        public string? Name { get; set; }
    }
} 