using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages.Admin
{
    public class SignOutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Remove("AdminUser");
            return RedirectToPage("/Index");
        }
    }
}