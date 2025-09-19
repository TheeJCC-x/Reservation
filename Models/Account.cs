using System.ComponentModel.DataAnnotations;

namespace Reservation.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
