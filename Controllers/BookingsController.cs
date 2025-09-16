using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // MAKE SURE THIS USING IS ADDED
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;
using Reservation.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult Create(DateTime? date, int? tableID)
        {
            ViewBag.TableList = new SelectList(_context.Tables.Where(t => t.Availability), "Id", "TableNumber");
            ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName");

            var model = new BookingViewModel();
            if (date.HasValue) model.BookingDate = date.Value;
            if (tableID.HasValue) model.TableId = tableID.Value;
            return View(model);

        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingDate,BookingTime,CustomerCount,CustomerName,CustomerPhoneNo,TableId,StaffId")] BookingViewModel booking)
        {
            if (ModelState.IsValid)
            {
                var conflict = _context.Bookings.Any(b => b.TableId == booking.TableId && b.BookingDate.Date == booking.BookingDate.Date);
                if (conflict)
                {
                    ModelState.AddModelError("", "This table is no longer availible.");

                    ViewBag.TableList = new SelectList(_context.Tables.Where(t => t.Availability), "Id", "TableNumber");
                    ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName");

                    return View(booking);
                }


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

        // Controller action to return a partial view for a given date
        [HttpGet]
        public async Task<IActionResult> DayLayout(DateTime date)
        {
            // Load all tables
            var tables = await _context.Tables.ToListAsync();

            // Load bookings for that date (day-level). If you need time-slot overlap checks, adjust later.
            var bookingsOnDate = await _context.Bookings
                .Where(b => b.BookingDate.Date == date.Date)
                .ToListAsync();

            // Build the DayLayoutViewModel
            var vm = new Reservation.Models.ViewModels.DayLayoutViewModel
            {
                Date = date
            };

            foreach (var t in tables.OrderBy(t => t.TableNumber))
            {
                var booked = bookingsOnDate.FirstOrDefault(b => b.TableId == t.Id);

                vm.Tables.Add(new Reservation.Models.ViewModels.TableAvailabilityDto
                {
                    TableID = t.Id,
                    TableNumber = t.TableNumber,
                    Seats = t.Seats,
                    IsAvailable = booked == null,
                    BookingId = booked?.Id
                });
            }

            // compute summary: available / total for each seat-size
            vm.SummaryBySeats = vm.Tables
                .GroupBy(x => x.Seats)
                .ToDictionary(
                    g => g.Key,
                    g => (available: g.Count(x => x.IsAvailable), total: g.Count())
                );

            // return partial view that is strongly-typed to DayLayoutViewModel
            return PartialView("_DayLayout", vm);
        }
    }
}
