using System.ComponentModel.DataAnnotations;
using GraceThreads.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GraceThreads.Pages
{
    public class IndexModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<GraceThreads.Models.User> _passwordHasher;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext db, IPasswordHasher<GraceThreads.Models.User> passwordHasher, ILogger<IndexModel> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If a user types your base website URL (e.g., cold visit to "https://localhost:7123/"),
            // immediately send them gracefully over to the shop storefront storefront page.
            if (Request.Path == "/")
            {
                return RedirectToPage("/Home");
            }

            // Otherwise, if they explicitly went to "/Index" or were forced here to log in,
            // let the page render normally.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await ValidateCredentialsAsync(Input.Email, Input.Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            TempData["WelcomeMessage"] = $"Welcome back, {user.DisplayName ?? user.Email}!";
            // Redirect based on role
            if (user.Role == 0)
            {
                return RedirectToPage("/Admin/Dashboard");
            }
            return RedirectToPage("Home");
        }
        private async Task<GraceThreads.Models.User?> ValidateCredentialsAsync(string email, string password)
        {
            email = email?.Trim().ToLowerInvariant() ?? string.Empty;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: no user found for email {Email}", email);
                return null;
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("Login failed: user {UserId} has empty PasswordHash", user.Id);
                return null;
            }

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            _logger.LogInformation("Password verification for user {UserId}: {Result}", user.Id, verify.ToString());
            if (verify == PasswordVerificationResult.Failed) return null;

            // Update LastLoginAt using database server time (SYSUTCDATETIME) to ensure DB-side timestamp
            try
            {
                await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE [Users] SET LastLoginAt = SYSUTCDATETIME() WHERE Id = {user.Id}");
                // Refresh the tracked entity to pick up DB-generated LastLoginAt
                await _db.Entry(user).ReloadAsync();
            }
            catch
            {
                // Fallback: set from application server time if DB update fails
                user.LastLoginAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
            }

            // Create claims and sign in using cookie authentication
            var roleName = user.Role == 0 ? "Admin" : "Customer";
            var lastLoginStr = user.LastLoginAt?.ToString("o") ?? string.Empty;

            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(System.Security.Claims.ClaimTypes.Name, user.DisplayName ?? user.Email),
                new(System.Security.Claims.ClaimTypes.Email, user.Email),
                new(System.Security.Claims.ClaimTypes.Role, roleName),
                new("LastLoginAt", lastLoginStr)
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            // Ensure authentication extension methods are available
            await this.HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return user;
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your password.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}