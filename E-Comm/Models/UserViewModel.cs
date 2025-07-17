using System.ComponentModel.DataAnnotations;

namespace E_Comm.Models
{
    public class UserViewModel
    {
        // User properties
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; } = false;
        
        // Password properties (for create/edit operations)
        [StringLength(255)]
        public string? Password { get; set; }
        
        [StringLength(255)]
        public string? ConfirmPassword { get; set; }
        
        // Customer properties (optional)
        public string? PhoneNumber { get; set; }
        
        // Australian address properties
        public string? StreetAddress { get; set; }
        public string? Suburb { get; set; }
        public string? State { get; set; }
        public int? PostCode { get; set; }
        
        // Payment information (optional)
        public string? CardNumber { get; set; }
        public string? CardOwner { get; set; }
        public string? Expiry { get; set; }
        public int? CVV { get; set; }
        
        // Helper properties
        public bool IsEdit { get; set; } = false;
        public bool HasCustomerProfile { get; set; } = false;
        
        // Convert to User model
        public User ToUser()
        {
            return new User
            {
                UserID = UserID,
                UserName = UserName,
                Email = Email,
                Name = Name,
                IsAdmin = IsAdmin
            };
        }
        
        // Convert to Customer model
        public Customer? ToCustomer()
        {
            if (string.IsNullOrEmpty(Email))
                return null;
                
            return new Customer
            {
                Email = Email,
                PhoneNumber = PhoneNumber,
                StreetAddress = StreetAddress,
                Suburb = Suburb,
                State = State,
                PostCode = PostCode,
                CardNumber = CardNumber,
                CardOwner = CardOwner,
                Expiry = Expiry,
                CVV = CVV
            };
        }

        // Create from User model
        public static UserViewModel FromUser(User user, Customer? customer = null)
        {
            var viewModel = new UserViewModel
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                IsAdmin = user.IsAdmin,
                IsEdit = true,
                HasCustomerProfile = customer != null
            };
            
            if (customer != null)
            {
                viewModel.PhoneNumber = customer.PhoneNumber;
                viewModel.StreetAddress = customer.StreetAddress;
                viewModel.Suburb = customer.Suburb;
                viewModel.State = customer.State;
                viewModel.PostCode = customer.PostCode;
                viewModel.CardNumber = customer.CardNumber;
                viewModel.CardOwner = customer.CardOwner;
                viewModel.Expiry = customer.Expiry;
                viewModel.CVV = customer.CVV;
            }
            
            return viewModel;
        }
    }

    // Activity models for admin dashboard
    public class ActivityItem
    {
        public string Type { get; set; } = "";
        public string Icon { get; set; } = "";
        public string IconColor { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; } = "";
    }

    public enum ActivityType
    {
        ProductAdded,
        ProductUpdated,
        ProductDeleted,
        UserRegistered,
        OrderPlaced,
        LowStockAlert,
        StockUpdated,
        UserCreated,
        UserUpdated
    }
} 