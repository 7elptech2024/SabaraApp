using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Data;
using Sabara.Models;
using Sabara.Web.ViewModel;

namespace Sabara.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProjectController : Controller
    {
        private const int PageSize = 8;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? category, int page = 1)
        {
            var query = _context.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            query = query.OrderByDescending(p => p.CreatedAt);

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Categories = await _context.Projects
                .Where(p => p.Category != null && p.Category != "")
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var paged = await PagedList<Project>.CreateAsync(query, page, PageSize);
            return View(paged);
        }

        public IActionResult Create() => View(new Project());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(project);

            if (imageFile != null && imageFile.Length > 0)
                project.ImagePath = await SaveImageAsync(imageFile);

            _context.Add(project);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إضافة المشروع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FindAsync(id);
            return project == null ? NotFound() : View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project, IFormFile? imageFile)
        {
            if (id != project.Id) return NotFound();
            if (!ModelState.IsValid) return View(project);

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var old = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (old != null && !string.IsNullOrEmpty(old.ImagePath))
                        DeleteImage(old.ImagePath);

                    project.ImagePath = await SaveImageAsync(imageFile);
                }

                _context.Update(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث المشروع بنجاح";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(p => p.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            return project == null ? NotFound() : View(project);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            return project == null ? NotFound() : View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                if (!string.IsNullOrEmpty(project.ImagePath))
                    DeleteImage(project.ImagePath);

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف المشروع";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var path = Path.Combine(uploads, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            return "/uploads/" + fileName;
        }

        private void DeleteImage(string relativePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
    }
}
