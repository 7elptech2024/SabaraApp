using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Web.Data;
using Sabara.Web.ViewModel;

namespace Sabara.Web.Controllers
{
    [Authorize(Roles = DbInitializer.AdminRole)]
    public class UsersController : Controller
    {
        private const int PageSize = 10;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? search, string? role, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Email!.Contains(search) || u.UserName!.Contains(search));

            var totalBeforeRole = await query.CountAsync();
            var pageUsers = await query.OrderBy(u => u.Email).ToListAsync();

            var items = new List<UserListItem>();
            foreach (var u in pageUsers)
            {
                var userRole = (await _userManager.GetRolesAsync(u)).FirstOrDefault() ?? "—";
                if (!string.IsNullOrWhiteSpace(role) && userRole != role) continue;
                items.Add(new UserListItem
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "",
                    Role = userRole,
                    IsCurrentUser = u.Id == currentUserId,
                });
            }

            var total = items.Count;
            if (page < 1) page = 1;
            var paged = items.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            var pagedList = new PagedList<UserListItem>(paged, total, page, PageSize);

            ViewBag.Search = search;
            ViewBag.RoleFilter = role;
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            ViewBag.TotalUsers = totalBeforeRole;
            ViewBag.AdminCount = (await _userManager.GetUsersInRoleAsync(DbInitializer.AdminRole)).Count;
            ViewBag.EmployeeCount = (await _userManager.GetUsersInRoleAsync(DbInitializer.EmployeeRole)).Count;

            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            if (!ModelState.IsValid) return View(model);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "هذا البريد مستخدم بالفعل");
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "الدور غير موجود");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["Success"] = "تم إنشاء المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Employee";
            return View(new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "",
                Role = role,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "الدور غير موجود");
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            // Prevent admin from demoting themselves
            if (user.Id == currentUserId && currentRole == DbInitializer.AdminRole && model.Role != DbInitializer.AdminRole)
            {
                ModelState.AddModelError(nameof(model.Role), "لا يمكنك تغيير دورك بنفسك");
                return View(model);
            }

            // Prevent removing the last admin
            if (currentRole == DbInitializer.AdminRole && model.Role != DbInitializer.AdminRole)
            {
                var admins = await _userManager.GetUsersInRoleAsync(DbInitializer.AdminRole);
                if (admins.Count <= 1)
                {
                    ModelState.AddModelError(nameof(model.Role), "لا يمكن إزالة آخر مدير في النظام");
                    return View(model);
                }
            }

            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (model.NewPassword.Length < 6)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), "كلمة المرور يجب أن تكون 6 خانات على الأقل");
                    return View(model);
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!resetResult.Succeeded)
                {
                    foreach (var e in resetResult.Errors) ModelState.AddModelError("", e.Description);
                    return View(model);
                }
            }

            TempData["Success"] = "تم تحديث المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "لا يمكنك حذف حسابك الحالي";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(DbInitializer.AdminRole))
            {
                var admins = await _userManager.GetUsersInRoleAsync(DbInitializer.AdminRole);
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "لا يمكن حذف آخر مدير في النظام";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "تم حذف المستخدم";
            return RedirectToAction(nameof(Index));
        }
    }
}
