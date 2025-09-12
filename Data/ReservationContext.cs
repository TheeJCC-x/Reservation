using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reservation.Models;

namespace Reservation.Data
{
    public class ReservationContext : DbContext
    {
        public ReservationContext (DbContextOptions<ReservationContext> options)
            : base(options)
        {
        }

        public DbSet<Reservation.Models.TableViewModel> TableViewModel { get; set; } = default!;
    }
}
