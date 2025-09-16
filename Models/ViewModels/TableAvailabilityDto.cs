namespace Reservation.Models.ViewModels
{
    public class TableAvailabilityDto
    {
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool IsAvailable { get; set; }
        public int? BookingId { get; set; }


    }
}
