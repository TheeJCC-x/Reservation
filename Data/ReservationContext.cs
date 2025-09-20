using Microsoft.EntityFrameworkCore;
using Reservation.Models;
using System.Collections.Generic;

// Updated by Byron (21/09/2025)

namespace Reservation.Data
{
    public class ReservationContext : DbContext
    {
        public ReservationContext(DbContextOptions<ReservationContext> options)
            : base(options)
        {
        }

        public DbSet<TableViewModel> Tables { get; set; } // <----- Restaurant Table
        public DbSet<BookingViewModel> Bookings { get; set; } // <----- Customer Bookings
        public DbSet<Staff> Staff { get; set; } // <----- Staff Members
        public DbSet<Transaction> Transactions { get; set; } // <----- Payment Transactions
        public DbSet<Account> Account { get; set; } // <----- User Accounts


        protected override void OnModelCreating(ModelBuilder modelBuilder) // Database Relationships
        {
            // -- Booking links to Table -- //
            modelBuilder.Entity<BookingViewModel>()
                .HasOne(b => b.Table)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TableId);
            // -- Booking links to Staff -- //
            modelBuilder.Entity<BookingViewModel>()
                .HasOne(b => b.Staff)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.StaffId);
            // -- Transaction links to Booking -- //
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BookingId);
            // -- Account table config -- //
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account"); // <----- Table Name
                entity.HasKey(a => a.Id); // <----- Primary Key
            });
        }
    }
}