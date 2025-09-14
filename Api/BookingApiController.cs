using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reservation.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingApiController : ControllerBase
    {
        private readonly ReservationContext _context;

        public BookingApiController(ReservationContext context)
        {
            _context = context;
        }

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
                    end = b.BookingDate.Add(b.BookingTime.TimeOfDay).AddHours(2),
                    extendedProps = new
                    {
                        customerName = b.CustomerName,
                        customerPhone = b.CustomerPhoneNo,
                        customerCount = b.CustomerCount,
                        tableNumber = b.Table.TableNumber
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingViewModel booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Create transaction
            var transaction = new Transaction
            {
                BookingId = booking.Id,
                TotalAmount = booking.CustomerCount * 50, // Adjust pricing as needed
                Status = "Created"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { id = booking.Id });
        }
    }
}