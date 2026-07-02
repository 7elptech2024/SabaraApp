using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Data;
using Sabara.Web.Models;
using Sabara.Web.ViewModel;

namespace Sabara.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class HomeAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var sevenDaysAgo = today.AddDays(-6);

            var totalOrders = await _context.Orders.CountAsync();
            var wonOrders = await _context.Orders.CountAsync(o => o.Stage == OrderStage.Won);
            var lostOrders = await _context.Orders.CountAsync(o => o.Stage == OrderStage.Lost);

            var vm = new DashboardViewModel
            {
                TotalProjects   = await _context.Projects.CountAsync(),
                TotalOrders     = totalOrders,
                PendingOrders   = await _context.Orders.CountAsync(o => !o.IsReplied),
                RepliedOrders   = await _context.Orders.CountAsync(o => o.IsReplied),
                OrdersToday     = await _context.Orders.CountAsync(o => o.CreatedAt >= today),
                OrdersThisWeek  = await _context.Orders.CountAsync(o => o.CreatedAt >= weekStart),
                OrdersThisMonth = await _context.Orders.CountAsync(o => o.CreatedAt >= monthStart),

                TotalClients     = await _context.Clients.CountAsync(),
                ActiveClients    = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Active),
                LeadClients      = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Lead),
                ClientsThisMonth = await _context.Clients.CountAsync(c => c.CreatedAt >= monthStart),
                WonOrders        = wonOrders,
                LostOrders       = lostOrders,
                ConversionPercent = totalOrders == 0 ? 0 : (int)((wonOrders / (double)totalOrders) * 100),

                RecentProjects = await _context.Projects
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                RecentOrders = await _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(6)
                    .ToListAsync(),

                RecentClients = await _context.Clients
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                ProjectsByCategory = await _context.Projects
                    .GroupBy(p => p.Category)
                    .Select(g => new CategorySlice { Category = g.Key ?? "غير مصنّف", Count = g.Count() })
                    .OrderByDescending(c => c.Count)
                    .ToListAsync(),

                OrdersByStage = await _context.Orders
                    .GroupBy(o => o.Stage)
                    .Select(g => new StageSlice { Stage = g.Key, Count = g.Count() })
                    .ToListAsync(),
            };

            var rawCounts = await _context.Orders
                .Where(o => o.CreatedAt >= sevenDaysAgo)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.OrdersLast7Days = Enumerable.Range(0, 7)
                .Select(i => sevenDaysAgo.AddDays(i))
                .Select(d => new DayPoint
                {
                    Date = d,
                    Count = rawCounts.FirstOrDefault(c => c.Date == d)?.Count ?? 0,
                })
                .ToList();

            return View(vm);
        }
    }
}
