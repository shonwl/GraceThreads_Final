using System.ComponentModel.DataAnnotations;
using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GraceThreads.Pages
{
    public class RegisterModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterModel(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            // Normalize email for comparison / storage
            var email = Input.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            // Check for existing email (case-insensitive)
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (existing != null)
            {
                ErrorMessage = "An account with that email already exists.";
                return Page();
            }

            var user = new User
            {
                Email = email,
                DisplayName = Input.FullName?.Trim(),
                PasswordHash = _passwordHasher.HashPassword(null, Input.Password),
                CreatedAt = DateTimeOffset.UtcNow,
                Role = 1
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Create claims and sign in the newly registered user
            var roleName = user.Role == 0 ? "Admin" : "Customer";
            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(System.Security.Claims.ClaimTypes.Name, user.DisplayName ?? user.Email),
                new(System.Security.Claims.ClaimTypes.Email, user.Email),
                new(System.Security.Claims.ClaimTypes.Role, roleName),
                new("LastLoginAt", string.Empty)
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            await this.HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["WelcomeMessage"] = $"Account created! Welcome, {user.DisplayName}.";
            return RedirectToPage("Home");
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your full name.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your email.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter a password.")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your password.")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}