using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            SuccessMessage = TempData["SuccessMessage"] as string;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Admin login check
            if (Input.Email.Equals("admin@gmail.com", StringComparison.OrdinalIgnoreCase) && Input.Password == "admin123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUser", "Admin User");
                return RedirectToPage("/Admin/Dashboard");
            }

            bool isValid = ValidateCredentials(Input.Email, Input.Password);

            if (!isValid)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            TempData["WelcomeMessage"] = $"Welcome back, {Input.Email}!";
            return RedirectToPage("Home");
        }

        private bool ValidateCredentials(string email, string password)
        {
            // TODO: replace with real authentication (EF Core / Identity / your API).
            return true;
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