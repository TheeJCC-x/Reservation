using System.ComponentModel.DataAnnotations;

namespace Reservation.Models
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }
        [DataType(DataType.Time)]
        public DateTime BookingTime { get; set; }
        public int CustomerCount { get; set; }
        public string? CustomerName { get; set; }
        [Phone]
        [MaxLength(15)]
        public string? CustomerPhoneNo { get; set; }

        // Foreign keys
        public int TableId { get; set; }
        public int? StaffId { get; set; }

        // Navigation properties
        public TableViewModel? Table { get; set; }
        public Staff? Staff { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}