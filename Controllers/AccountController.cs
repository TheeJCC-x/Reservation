using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;
using Reservation.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Reservation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ReservationContext _authService;
        public AccountController(ReservationContext authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var isValid = await _authService.Account.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (isValid == null)
            {
                return View();
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //var authProperties = new AuthenticationProperties
            //{
            //    //IsPersistent = _authService.Account.RememberMe,
            //    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            //};

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
                );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
