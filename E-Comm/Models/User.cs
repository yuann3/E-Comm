using System.ComponentModel.DataAnnotations;

namespace E_Comm.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;
        
        [StringLength(255)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(255)]
        public string? Name { get; set; }
        
        public bool IsAdmin { get; set; } = false;
        
        [StringLength(32)]
        public string? Salt { get; set; }
        
        [StringLength(64)]
        public string? HashPW { get; set; }
        

        public bool IsEmployee => !IsAdmin && !string.IsNullOrEmpty(UserName);
        public bool IsCustomer => !IsAdmin && !IsEmployee;
    }
} 