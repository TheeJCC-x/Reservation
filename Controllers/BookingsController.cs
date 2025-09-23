using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Bookings
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;

            var bookings = _context.Bookings
                                  .Include(b => b.Table)
                                  .Include(b => b.Staff)
                                  .AsQueryable();

            if (string.IsNullOrEmpty(sortOrder))
            {
                bookings = bookings.OrderBy(b => b.BookingDate).ThenBy(b => b.BookingTime);
            }
            else if (sortOrder == "name_asc")
            {
                bookings = bookings.OrderBy(b => b.CustomerName);
            }
            else if (sortOrder == "name_desc")
            {
                bookings = bookings.OrderByDescending(b => b.CustomerName);
            }

            return View(await bookings.ToListAsync());
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

        //  Added by Jene (22/09/25) - Added code for details, edit, delete & Search Feature

        // GET: Bookings Search Function
        public async Task<IActionResult> IndexSearch(string searchString)
        {
            if (_context.Bookings == null)      // <--- checks if the Bookings in the database is null
            {
                return Problem("Entity set 'BookingContext.Bookings' is null");     // <--- return an error if null
            }

            var booking = from b in _context.Bookings       // <--- create LINQ query that selects all bookings
                          select b;
            if(!String.IsNullOrEmpty(searchString))         // <--- check if user entered a search string
            {
                // find the matching booking name
                booking = booking.Where(s => s.CustomerName!.ToUpper().Contains(searchString.ToUpper()));
            }
            return View(await booking.ToListAsync());      // <--- execute query and return the results
        }

        //[HttpPost]
        public string Search(string searchString, bool notUsed)  // <--- overload index method
        {
            return "From [HttpPost] Index: filter on" + searchString;   // <--- returns message
        }

        // GET: Bookings/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Staff)
                .Include(b => b.Table)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName", booking.StaffId);
            ViewBag.TableList = new SelectList(_context.Tables, "Id", "TableNumber", booking.TableId);

            return View(booking);
        }

        // POST: Bookings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BookingDate,BookingTime,CustomerCount,CustomerName,CustomerPhoneNo,TableId,StaffId")] BookingViewModel booking)
        {
            if (id != booking.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bookings.Any(e => e.Id == booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StaffList = new SelectList(_context.Staff, "StaffId", "StaffName", booking.StaffId);
            ViewBag.TableList = new SelectList(_context.Tables, "Id", "TableNumber", booking.TableId);

            return View(booking);
        }

        // GET: Bookings/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Staff)
                .Include(b => b.Table)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}