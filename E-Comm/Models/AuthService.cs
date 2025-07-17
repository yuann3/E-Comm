using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace E_Comm.Models
{
    /// <summary>
    /// Handles user authentication and registration services.
    /// Supports both test credentials and database-backed users.
    /// </summary>
    public class AuthService
    {
        private readonly EntertainmentGuildContext _context;

        public AuthService(EntertainmentGuildContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Predefined test credentials for development and testing.
        /// </summary>
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

        /// <summary>
        /// Attempts to authenticate a user by email and password.
        /// First checks test credentials, then queries the database.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain-text password</param>
        /// <returns>Authentication result with user details and status</returns>
        public async Task<AuthResult> AuthenticateAsync(string email, string password)
        {
            // Check hardcoded test credentials
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

            // Query the database for the user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Verify password and return result
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

            // Default failure response
            return new AuthResult { IsSuccess = false };
        }

        /// <summary>
        /// Registers a new user after checking for existing emails and usernames.
        /// </summary>
        /// <param name="user">User details</param>
        /// <param name="password">User's password (plain text)</param>
        /// <returns>Registration result indicating success or error</returns>
        public async Task<RegistrationResult> RegisterUserAsync(User user, string password)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null || _testCredentials.ContainsKey(user.Email ?? ""))
                {
                    return new RegistrationResult { IsSuccess = false, ErrorMessage = "Email is already registered." };
                }

                // Check if username is taken
                var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
                if (existingUsername != null)
                {
                    return new RegistrationResult { IsSuccess = false, ErrorMessage = "Username is already taken." };
                }

                // Generate salt and hashed password
                user.Salt = GenerateSalt();
                user.HashPW = HashPassword(password, user.Salt);

                // Set default role for new users
                user.IsAdmin = false;

                // Save new user
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new RegistrationResult { IsSuccess = true };
            }
            catch (Exception)
            {
                return new RegistrationResult { IsSuccess = false, ErrorMessage = "Registration failed. Please try again." };
            }
        }

        /// <summary>
        /// Verifies a password against stored salt and hash.
        /// </summary>
        private bool VerifyPassword(string password, string? salt, string? hashedPassword)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hashedPassword))
                return false;

            var hash = HashPassword(password, salt);
            return hash == hashedPassword;
        }

        /// <summary>
        /// Hashes a password using SHA256 and salt.
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        /// <summary>
        /// Generates a secure random salt for password hashing.
        /// </summary>
        private string GenerateSalt()
        {
            var saltBytes = new byte[24];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
    }

    /// <summary>
    /// Structure for hardcoded user credentials used in development/testing.
    /// </summary>
    public class UserCredentials
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// Result returned from an authentication attempt.
    /// Includes role and flags used in business logic.
    /// </summary>
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

    /// <summary>
    /// Result returned from a registration attempt.
    /// Contains status and an optional error message.
    /// </summary>
    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
