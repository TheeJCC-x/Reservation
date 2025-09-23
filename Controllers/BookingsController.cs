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
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            // Start IQueryable so we can apply filters & sorts before querying DB
            var query = _context.Bookings
                        .Include(b => b.Table)
                        .Include(b => b.Staff)
                        .AsQueryable();

            // Filter (search)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(b => b.CustomerName != null && b.CustomerName.Contains(s));
            }

            // Sort
            if (string.IsNullOrEmpty(sortOrder))
            {
                query = query.OrderBy(b => b.BookingDate).ThenBy(b => b.BookingTime);
            }
            else if (sortOrder == "name_asc")
            {
                query = query.OrderBy(b => b.CustomerName);
            }
            else if (sortOrder == "name_desc")
            {
                query = query.OrderByDescending(b => b.CustomerName);
            }

            // Project entity -> view model (so your view uses BookingViewModel safely)
            var model = await query
                .Select(b => new BookingViewModel
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    BookingTime = b.BookingTime,
                    CustomerCount = b.CustomerCount,
                    CustomerName = b.CustomerName,
                    CustomerPhoneNo = b.CustomerPhoneNo,
                    TableId = b.TableId,
                    StaffId = b.StaffId,
                    Table = b.Table,   // if your VM has navigation props; otherwise map specific fields
                    Staff = b.Staff
                })
                .ToListAsync();

            return View(model);
        }


        // Separate action for sorting (keeps searchString so sorting a filtered list works)
        // Call this from your view's sort links (asp-action="Sort")
        public async Task<IActionResult> Sort(string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;

            var bookings = _context.Bookings
                                   .Include(b => b.Table)
                                   .Include(b => b.Staff)
                                   .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                bookings = bookings.Where(b => b.CustomerName != null && b.CustomerName.Contains(s));
            }

            // Apply requested sort
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

            return View("Index", await bookings.ToListAsync());
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

        // GET: Bookings Search Function (you can keep this if you want a separate search page)
        public async Task<IActionResult> IndexSearch(string searchString)
        {
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'BookingContext.Bookings' is null");
            }

            var booking = from b in _context.Bookings
                          select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                booking = booking.Where(s => s.CustomerName!.ToUpper().Contains(searchString.ToUpper()));
            }
            return View(await booking.ToListAsync());
        }

        //[HttpPost]
        public string Search(string searchString, bool notUsed)
        {
            return "From [HttpPost] Index: filter on" + searchString;
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
