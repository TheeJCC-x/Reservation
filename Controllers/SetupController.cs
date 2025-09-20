using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;

public class SetupController : Controller
{
    private readonly ReservationContext _context;

    public SetupController(ReservationContext context)
    {
        _context = context;
    }

    public IActionResult CreateAdmin()
    {
        // Check if admin already exists
        if (!_context.Account.Any(a => a.Username == "admin"))
        {
            string adminHash = BCrypt.Net.BCrypt.HashPassword("password");

            _context.Account.Add(new Account
            {
                Username = "admin",
                Password = adminHash,
                RememberMe = false
            });

            _context.SaveChanges();
            return Content("Admin account created successfully! Username: admin, Password: password");
        }

        return Content("Admin account already exists.");
    }

    public IActionResult ViewAllAccounts()
    {
        var accounts = _context.Account.ToList();
        return Json(accounts);
    }
}