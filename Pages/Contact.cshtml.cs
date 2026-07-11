using System.ComponentModel.DataAnnotations;
using GraceThreads.Models;
using GraceThreads.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public List<CartItem> CartItems { get; set; } = new();

        public void OnGet()
        {
            SuccessMessage = TempData["ContactSuccessMessage"] as string;
            CartItems = CartService.GetCart(HttpContext.Session);
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                CartItems = CartService.GetCart(HttpContext.Session);
                return Page();
            }

            // TODO: replace with real email sending / ticket creation / DB save.
            SendMessage(Input.FullName, Input.Email, Input.Subject, Input.Message);

            TempData["ContactSuccessMessage"] = "Thanks for reaching out! We'll get back to you within 24–48 hours.";
            return RedirectToPage("Contact");
        }

        private void SendMessage(string fullName, string email, string subject, string message)
        {
            // Placeholder — wire this to your real mail/notification service.
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your full name.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your email.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter a subject.")]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter a message.")]
            [StringLength(1000, ErrorMessage = "Message must be under {1} characters.")]
            public string Message { get; set; } = string.Empty;
        }
    }
}