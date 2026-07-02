using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sabara.Data;
using Sabara.Models;
using Sabara.Web.ViewModel;

namespace Sabara.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _logger = logger;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult ServiceWebsite() => View();
        public IActionResult ServiceWebsiteDesign() => View();
        public IActionResult ServiceVideoDesign() => View();
        public IActionResult ServiceSocialMediaAds() => View();
        public IActionResult ServicePrintAdDesign() => View();
        public IActionResult ServiceContent() => View();
        public IActionResult ServiceCommentary() => View();
        public IActionResult ServiceBrandDesign() => View();

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects.ToListAsync();
            return View(projects);
        }

        public IActionResult Privacy() => View();

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "HomeAdmin");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                ViewBag.LoginError = "تأكد من صحة البيانات.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "HomeAdmin");
            }

            ViewBag.LoginError = "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
