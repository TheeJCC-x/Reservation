using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Reservation.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }      //e.g. Created, Updated, Cancelled 

        // Foreign key - rename for consistency
        public int BookingId { get; set; }

        // Navigation property
        public BookingViewModel Booking { get; set; }
    }
}
