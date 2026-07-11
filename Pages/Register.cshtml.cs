using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: replace with real persistence (EF Core / Identity / your API).
            bool created = CreateAccount(Input.FullName, Input.Email, Input.Password);

            if (!created)
            {
                ErrorMessage = "An account with that email already exists.";
                return Page();
            }

            TempData["WelcomeMessage"] = $"Account created! Welcome, {Input.FullName}.";
            return RedirectToPage("Home");
        }

        private bool CreateAccount(string fullName, string email, string password)
        {
            // Placeholder — wire this to your real database/user store.
            return true;
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