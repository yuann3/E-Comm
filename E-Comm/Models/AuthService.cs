using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace E_Comm.Models
{
    public class AuthService
    {
        private readonly EntertainmentGuildContext _context;

        public AuthService(EntertainmentGuildContext context)
        {
            _context = context;
        }

        // Test credentials from requirements document
        private readonly Dictionary<string, UserCredentials> _testCredentials = new()
        {
            {
                "customer@example.com", 
                new UserCredentials { Email = "customer@example.com", Password = "Password1", Role = "Customer", Name = "Cus Tomer" }
            },
            {
                "employee@example.com", 
                new UserCredentials { Email = "employee@example.com", Password = "Passw0rd", Role = "Employee", Name = "Empl Oyee" }
            },
            {
                "administrator@example.com", 
                new UserCredentials { Email = "administrator@example.com", Password = "Pa$$w0rd", Role = "Admin", Name = "Admin Istrator" }
            }
        };

        public async Task<AuthResult> AuthenticateAsync(string email, string password)
        {
            // First try test credentials for development
            if (_testCredentials.ContainsKey(email))
            {
                var testUser = _testCredentials[email];
                if (testUser.Password == password)
                {
                    return new AuthResult
                    {
                        IsSuccess = true,
                        Email = testUser.Email,
                        Name = testUser.Name,
                        Role = testUser.Role,
                        IsAdmin = testUser.Role == "Admin",
                        IsEmployee = testUser.Role == "Employee",
                        IsCustomer = testUser.Role == "Customer"
                    };
                }
            }

            // Then try database authentication
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && VerifyPassword(password, user.Salt, user.HashPW))
            {
                return new AuthResult
                {
                    IsSuccess = true,
                    Email = user.Email ?? "",
                    Name = user.Name ?? "",
                    Role = user.IsAdmin ? "Admin" : "Employee",
                    IsAdmin = user.IsAdmin,
                    IsEmployee = !user.IsAdmin,
                    IsCustomer = false
                };
            }

            return new AuthResult { IsSuccess = false };
        }

        private bool VerifyPassword(string password, string? salt, string? hashedPassword)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hashedPassword))
                return false;

            var hash = HashPassword(password, salt);
            return hash == hashedPassword;
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }

    public class UserCredentials
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsCustomer { get; set; }
    }
} 