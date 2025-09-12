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

        //link to reservation one to one
        public int ID { get; set; }
        public BookingViewModel BookingViewModel { get; set; }
    }
}
