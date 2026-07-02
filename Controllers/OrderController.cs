using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Data;
using Sabara.Web.Models;
using Sabara.Web.ViewModel;

namespace Sabara.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class OrderController : Controller
    {
        private const int PageSize = 10;

        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            var query = _context.Orders.Include(o => o.Client).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o =>
                    o.CustomerName.Contains(search) ||
                    o.PhoneNumber.Contains(search) ||
                    (o.ServiceName != null && o.ServiceName.Contains(search)));

            if (status == "pending") query = query.Where(o => !o.IsReplied);
            else if (status == "replied") query = query.Where(o => o.IsReplied);

            query = query.OrderByDescending(o => o.CreatedAt);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PendingCount = await _context.Orders.CountAsync(o => !o.IsReplied);
            ViewBag.RepliedCount = await _context.Orders.CountAsync(o => o.IsReplied);
            ViewBag.TotalCount = await _context.Orders.CountAsync();

            var paged = await PagedList<Order>.CreateAsync(query, page, PageSize);
            return View(paged);
        }

        public async Task<IActionResult> Pipeline()
        {
            var orders = await _context.Orders
                .Include(o => o.Client)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStage(int id, OrderStage stage)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Stage = stage;
            if (stage != OrderStage.New) order.IsReplied = true;
            await _context.SaveChangesAsync();

            if (Request.Headers.ContainsKey("X-Requested-With"))
                return Json(new { ok = true, stage = stage.ToString() });

            return RedirectToAction(nameof(Pipeline));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReplied(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order != null && !order.IsReplied)
            {
                order.IsReplied = true;
                if (order.Stage == OrderStage.New) order.Stage = OrderStage.Contacted;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تأكيد الرد على الطلب";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToClient(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.ClientId.HasValue)
            {
                TempData["Error"] = "هذا الطلب مرتبط بعميل بالفعل";
                return RedirectToAction(nameof(Pipeline));
            }

            var existing = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == order.PhoneNumber);
            if (existing != null)
            {
                order.ClientId = existing.Id;
                if (order.Stage != OrderStage.Lost) order.Stage = OrderStage.Won;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"تم ربط الطلب بالعميل الموجود: {existing.Name}";
                return RedirectToAction("Details", "Clients", new { id = existing.Id });
            }

            var client = new Client
            {
                Name = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Notes = order.Notes,
                Status = ClientStatus.Active,
                Source = ClientSource.Website,
                CreatedAt = DateTime.Now,
                LastContactAt = DateTime.Now,
            };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            order.ClientId = client.Id;
            order.Stage = OrderStage.Won;
            order.IsReplied = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحويل الطلب إلى عميل جديد";
            return RedirectToAction("Details", "Clients", new { id = client.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الطلب";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
                TempData["OrderSuccess"] = true;
                return RedirectToAction("Index", "Home", new { section = "pricing" });
            }

            TempData["OrderError"] = "يرجى التحقق من إدخال البيانات بشكل صحيح.";
            return RedirectToAction("Index", "Home", new { section = "pricing" });
        }
    }
}
