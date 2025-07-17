using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using E_Comm.Models;

namespace E_Comm.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var authResult = await _authService.AuthenticateAsync(email, password);

            if (authResult.IsSuccess)
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, authResult.Email),
                    new Claim(ClaimTypes.Name, authResult.Name),
                    new Claim(ClaimTypes.Role, authResult.Role),
                    new Claim("IsAdmin", authResult.IsAdmin.ToString()),
                    new Claim("IsEmployee", authResult.IsEmployee.ToString()),
                    new Claim("IsCustomer", authResult.IsCustomer.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect based on user role (Business Scenario requirement)
                if (authResult.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (authResult.IsEmployee)
                {
                    return RedirectToAction("Index", "Employee");
                }
                else if (authResult.IsCustomer)
                {
                    return RedirectToAction("Index", "Customer");
                }

                // Default redirect if no specific role
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // Sign Up Method
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        // Sign Up Logic
        [HttpPost]
        public async Task<IActionResult> Signup(User model, string password, string confirmPassword)
        {
            // Basic validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Password is required.";
                return View(model);
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(model);
            }
            try
            {
                var result = await _authService.RegisterUserAsync(model, password);
                if (result.IsSuccess)
                {
                    // Auto-login user after successful registration
                    var authResult = await _authService.AuthenticateAsync(model.Email, password);
                    if (authResult.IsSuccess)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, authResult.Email),
                            new Claim(ClaimTypes.Name, authResult.Name),
                            new Claim(ClaimTypes.Role, authResult.Role),
                            new Claim("IsAdmin", authResult.IsAdmin.ToString()),
                            //new Claim("IsEmployee", authResult.IsEmployee.ToString()),
                            new Claim("IsCustomer", authResult.IsCustomer.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        // Redirect new users to page
                        return RedirectToAction("Index", "Home");
                    }

                    // If auto-login fails, redirect to Login page
                    TempData["Success"] = "Registration is successful! You may now log in.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = result.ErrorMessage ?? "Registration failed. Try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred during registration. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 