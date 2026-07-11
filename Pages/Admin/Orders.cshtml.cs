using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public OrdersModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Order> Orders { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items!).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return Page();
        }
    }
}
