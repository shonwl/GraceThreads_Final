using System.Threading.Tasks;
using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Pages.Admin
{
    // Development-only page to check stored password hash and verification result for troubleshooting.
    public class DevCheckCredentialsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IWebHostEnvironment _env;

        public DevCheckCredentialsModel(ApplicationDbContext db, IPasswordHasher<User> hasher, IWebHostEnvironment env)
        {
            _db = db;
            _hasher = hasher;
            _env = env;
        }

        [BindProperty]
        public string Email { get; set; } = "admin@gracethreads.com";

        [BindProperty]
        public string Password { get; set; } = "admin123";

        public string? ResultMessage { get; set; }

        public void OnGet()
        {
            if (!_env.IsDevelopment())
            {
                Response.StatusCode = 404;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_env.IsDevelopment()) return NotFound();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == (Email ?? string.Empty).Trim().ToLowerInvariant());
            if (user == null)
            {
                ResultMessage = "User not found.";
                return Page();
            }

            var hashLen = string.IsNullOrEmpty(user.PasswordHash) ? 0 : user.PasswordHash.Length;
            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, Password ?? string.Empty);

            ResultMessage = $"UserId={user.Id}; Email={user.Email}; HashLength={hashLen}; VerifyResult={verify}; LastLoginAt={user.LastLoginAt?.ToString("o") ?? "(null)"}";
            return Page();
        }
    }
}
