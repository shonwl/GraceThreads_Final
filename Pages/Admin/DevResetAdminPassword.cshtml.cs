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
    // Development-only page to reset or create the seeded admin password.
    // THIS PAGE SHOULD NOT BE LEFT IN PRODUCTION.
    public class DevResetAdminPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IWebHostEnvironment _env;

        public DevResetAdminPasswordModel(ApplicationDbContext db, IPasswordHasher<User> hasher, IWebHostEnvironment env)
        {
            _db = db;
            _hasher = hasher;
            _env = env;
        }

        [BindProperty]
        public string NewPassword { get; set; } = "admin123";

        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            if (!_env.IsDevelopment()) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_env.IsDevelopment()) return NotFound();

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                Message = "Password must be at least 6 characters.";
                return Page();
            }

            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == "admin@gracethreads.com");
            if (admin == null)
            {
                admin = new User
                {
                    Email = "admin@gracethreads.com",
                    DisplayName = "Administrator",
                    Role = 0,
                    CreatedAt = System.DateTimeOffset.UtcNow
                };
                admin.PasswordHash = _hasher.HashPassword(admin, NewPassword);
                _db.Users.Add(admin);
                await _db.SaveChangesAsync();
                Message = "Admin user created and password set.";
                return Page();
            }
            admin.PasswordHash = _hasher.HashPassword(admin, NewPassword);
            await _db.SaveChangesAsync();
            Message = "Admin password updated.";
            return Page();
        }
    }
}
