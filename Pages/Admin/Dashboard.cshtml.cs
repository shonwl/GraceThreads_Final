using GraceThreads.Data;
using GraceThreads.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GraceThreads.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public decimal TotalRevenue { get; private set; }
        public int TotalOrders { get; private set; }
        public int TotalProducts { get; private set; }
        public int ActiveListings { get; private set; }

        public List<Order> RecentOrders { get; private set; } = new();
        public List<Product> RecentProducts { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            TotalRevenue = await _db.Orders.SumAsync(o => (decimal?)o.Total) ?? 0m;
            TotalOrders = await _db.Orders.CountAsync();
            TotalProducts = await _db.Products.CountAsync();
            ActiveListings = await _db.Products.CountAsync(p => p.Active);

            RecentOrders = await _db.Orders
                .OrderByDescending(o => o.Date)
                .Take(3)
                .Include(o => o.User)
                .ToListAsync();

            RecentProducts = await _db.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}
