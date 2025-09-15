namespace Reservation.Models
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool Availability { get; set; }

        public ICollection<BookingViewModel>? Bookings { get; set; }
    }
}