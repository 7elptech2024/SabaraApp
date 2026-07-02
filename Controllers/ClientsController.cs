using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Data;
using Sabara.Web.Models;
using Sabara.Web.ViewModel;

namespace Sabara.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ClientsController : Controller
    {
        private const int PageSize = 12;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, ClientStatus? status, int page = 1)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c =>
                    c.Name.Contains(s) ||
                    c.PhoneNumber.Contains(s) ||
                    (c.CompanyName != null && c.CompanyName.Contains(s)) ||
                    (c.Email != null && c.Email.Contains(s)));
            }

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            ViewBag.Search = search;
            ViewBag.StatusFilter = status;
            ViewBag.TotalCount = await _context.Clients.CountAsync();
            ViewBag.LeadCount = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Lead);
            ViewBag.ActiveCount = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Active);
            ViewBag.InactiveCount = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Inactive);
            ViewBag.LostCount = await _context.Clients.CountAsync(c => c.Status == ClientStatus.Lost);

            var paged = await PagedList<Client>.CreateAsync(query, page, PageSize);
            return View(paged);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            ViewBag.AssignedUser = !string.IsNullOrEmpty(client.AssignedToUserId)
                ? (await _userManager.FindByIdAsync(client.AssignedToUserId))?.Email
                : null;

            return View(client);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateAssignableUsersAsync();
            return View(new Client());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAssignableUsersAsync();
                return View(client);
            }
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة العميل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            await PopulateAssignableUsersAsync();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateAssignableUsersAsync();
                return View(client);
            }

            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات العميل";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clients.Any(c => c.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف العميل";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.LastContactAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث وقت آخر تواصل";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateAssignableUsersAsync()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            ViewBag.AssignableUsers = users.Select(u => new { u.Id, Email = u.Email ?? u.UserName ?? "" }).ToList();
        }
    }
}
