using System.ComponentModel.DataAnnotations;

namespace Reservation.Models
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]                       //represents a date value
        public DateTime BookingDate { get; set; }
        [DataType(DataType.Time)]                       //represents a time value
        public DateTime BookingTime { get; set; }
        public int CustomerCount { get; set; }
        public string? CustomerName { get; set; }       // ? = property is nullable
        [Phone]
        [MaxLength(15)]
        public string? CustomerPhoneNo { get; set; }

        public ICollection<Transaction> Transaction { get; set; }

    }//end of class

}//end of nmspc
