namespace Reservation.Models
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool Availability { get; set; }

        // many reservations can link this table
        public ICollection<BookingViewModel> Bookings { get; set; }

    }//end of class

}//end of nmspc
