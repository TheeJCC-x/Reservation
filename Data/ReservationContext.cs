using Microsoft.EntityFrameworkCore;
using Reservation.Models;
using System.Collections.Generic;

namespace Reservation.Data
{
    public class ReservationContext : DbContext
    {
        public ReservationContext(DbContextOptions<ReservationContext> options)
            : base(options)
        {
        }

        public DbSet<TableViewModel> Tables { get; set; }
        public DbSet<BookingViewModel> Bookings { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<BookingViewModel>()
                .HasOne(b => b.Table)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TableId);

            modelBuilder.Entity<BookingViewModel>()
                .HasOne(b => b.Staff)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.StaffId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BookingId);

            // Seed data for Tables
            modelBuilder.Entity<TableViewModel>().HasData(
                new TableViewModel { Id = 1, TableNumber = 1, Seats = 2, Availability = true },
                new TableViewModel { Id = 2, TableNumber = 2, Seats = 2, Availability = true },
                new TableViewModel { Id = 3, TableNumber = 3, Seats = 4, Availability = true },
                new TableViewModel { Id = 4, TableNumber = 4, Seats = 4, Availability = true },
                new TableViewModel { Id = 5, TableNumber = 5, Seats = 6, Availability = true },
                new TableViewModel { Id = 6, TableNumber = 6, Seats = 6, Availability = true },
                new TableViewModel { Id = 7, TableNumber = 7, Seats = 8, Availability = true },
                new TableViewModel { Id = 8, TableNumber = 8, Seats = 8, Availability = true }
            );

            // Seed data for Staff
            modelBuilder.Entity<Staff>().HasData(
                new Staff { StaffId = 1, StaffName = "Byron" },
                new Staff { StaffId = 2, StaffName = "Jene" },
                new Staff { StaffId = 3, StaffName = "Jiteesh" }
            );
        }
    }
}