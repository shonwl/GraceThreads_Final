using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraceThreads.Pages.Admin
{
    public class OrdersModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }
    }
}