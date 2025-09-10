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

        //staff can create many bookings
        public ICollection<BookingViewModel> BookingViewModel { get; set; }

    }
}
