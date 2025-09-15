using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // MAKE SURE THIS USING IS ADDED
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Reservation.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ReservationContext _context;

        public BookingsController(ReservationContext context)
        {
            _context = context;
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            // Populate ViewBag with SelectLists for dropdowns
            ViewBag.TableList = new SelectList(_context.Tables.Where(t => t.Availability), "Id", "TableNumber");
            ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName");

            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingDate,BookingTime,CustomerCount,CustomerName,CustomerPhoneNo,TableId,StaffId")] BookingViewModel booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                // Create transaction
                var transaction = new Transaction
                {
                    BookingId = booking.Id,
                    TotalAmount = CalculateTotalAmount(booking.CustomerCount),
                    Status = "Created"
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // If model is invalid, repopulate the dropdowns
            ViewBag.TableList = new SelectList(_context.Tables.Where(t => t.Availability), "Id", "TableNumber");
            ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName");

            return View(booking);
        }

        private decimal CalculateTotalAmount(int customerCount)
        {
            return customerCount * 50; // $50 per person
        }

        // API endpoint for FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Table)
                .Select(b => new
                {
                    id = b.Id,
                    title = $"{b.CustomerName} - Table {b.Table.TableNumber}",
                    start = b.BookingDate.Add(b.BookingTime.TimeOfDay),
                    end = b.BookingDate.Add(b.BookingTime.TimeOfDay).AddHours(2), // 2-hour booking
                    customerName = b.CustomerName,
                    customerPhone = b.CustomerPhoneNo,
                    customerCount = b.CustomerCount,
                    tableNumber = b.Table.TableNumber
                })
                .ToListAsync();

            return Ok(bookings);
        }
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Table)
                .Include(b => b.Staff)
                .ToListAsync();

            return View(bookings);
        }
    }
}