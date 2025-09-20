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

// Updated by Byron (21/09/2025)

namespace Reservation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ReservationContext _authService; // <---- Database Connection
        public AccountController(ReservationContext authService) // <---- Constructor
        {
            _authService = authService; // <---- Inject the database context
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null) // <---- Show the login page
        {
            ViewData["ReturnUrl"] = returnUrl; // <---- Remember the redirection URL when user logs in
            return View(); // <---- Return to the login page
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // <---- Store the return URL

            // First find user by username only
            var user = await _authService.Account
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) // <---- If the user isn't found
            {
                ModelState.AddModelError("", "Invalid login attempt."); // <---- Wrong Password
                return View(); // <---- Return to the login page
            }

            // Then verify the password against the hash
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }
            // Create user identity claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
             
            await HttpContext.SignInAsync( // <------ Sign in the user and create cookie
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) // <------ Redirect to original requested page or home
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home"); // <------ Redirect user to the homepage (Dashboard)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() // <------ User logout
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // <------ Remove the auth cookie
            return RedirectToAction("Login", "Account"); // <------ Go back to the login page
        }

        public IActionResult AccessDenied() // <------ If the user doesn't have permissions
        {
            return View(); // <------ Show access denied page 
        }

        public IActionResult DebugAccounts() // <------ Debugging for checking stored accounts
        {
            var accounts = _authService.Account.ToList(); // <------ Get all accounts from database
            return Json(accounts); // <------ Show the accounts as JSON data
        }

        public IActionResult DebugDatabase() // <------ Debugging for checking database info
        {
            try
            {
                // Get the database connection information
                var connectionString = _authService.Database.GetDbConnection().ConnectionString;
                var databaseName = _authService.Database.GetDbConnection().Database;

                return Content($"Connection: {connectionString}\nDatabase: {databaseName}"); // <------ Show the connection information
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}"); // <------ Show errors
            }
        }
    }
}