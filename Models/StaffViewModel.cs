using System.ComponentModel.DataAnnotations;

namespace Reservation.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StaffName { get; set; }

        // Rename for clarity (plural since it's a collection)
        public ICollection<BookingViewModel> Bookings { get; set; }
    }
}
