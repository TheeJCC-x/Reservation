using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservation.Data;
using Reservation.Models;
using System.Threading.Tasks;
using System; 

// Updated by Byron (21/09/2025)

namespace Reservation.Seeding
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider) // <------ Seeding Method
        {
            using (var context = new ReservationContext(
                serviceProvider.GetRequiredService<DbContextOptions<ReservationContext>>()))
            {
                Console.WriteLine("Starting database seeding..."); // <------ Start Seeding Message

                // --- Accounts --- //
                if (!await context.Account.AnyAsync(a => a.Username == "admin")) // <------ Check if admin exists
                {
                    string adminPasswordHashed = BCrypt.Net.BCrypt.HashPassword("password"); // <------ Hash the admin password
                    // -- Add Admin Account //
                    context.Account.Add(new Account
                    {
                        Username = "admin",
                        Password = adminPasswordHashed, // <------ Hashed Admin Password
                        RememberMe = false
                    });
                }

                if (!await context.Account.AnyAsync(a => a.Username == "staff")) // <------ Check if staff exists
                {
                    string staffPasswordHashed = BCrypt.Net.BCrypt.HashPassword("password1"); // <------ Hash the staff password
                    // -- Add Staff Account //
                    context.Account.Add(new Account
                    {
                        Username = "staff",
                        Password = staffPasswordHashed, // <------ Hashed Staff Password
                        RememberMe = false
                    });
                }

                // --- Tables --- //
                if (!await context.Tables.AnyAsync()) // <------ Check if the table exists
                {
                    // -- Add Tables //
                    context.Tables.AddRange(
                        new TableViewModel { Id = 1, Availability = true, Seats = 2, TableNumber = 1 },
                        new TableViewModel { Id = 2, Availability = true, Seats = 2, TableNumber = 2 },
                        new TableViewModel { Id = 3, Availability = true, Seats = 4, TableNumber = 3 },
                        new TableViewModel { Id = 4, Availability = true, Seats = 4, TableNumber = 4 },
                        new TableViewModel { Id = 5, Availability = true, Seats = 6, TableNumber = 5 },
                        new TableViewModel { Id = 6, Availability = true, Seats = 6, TableNumber = 6 },
                        new TableViewModel { Id = 7, Availability = true, Seats = 8, TableNumber = 7 },
                        new TableViewModel { Id = 8, Availability = true, Seats = 8, TableNumber = 8 }
                    );
                }

                // --- Staff --- //
                if (!await context.Staff.AnyAsync())
                {
                    // -- Add Staff //
                    context.Staff.AddRange(
                        new Staff { StaffId = 1, StaffName = "Jiteesh" },
                        new Staff { StaffId = 2, StaffName = "Byron" },
                        new Staff { StaffId = 3, StaffName = "Jene" }
                    );
                }

                // --- Bookings --- //
                if (!await context.Bookings.AnyAsync())
                {
                    // -- Add Bookings //
                    context.Bookings.AddRange(
                        new BookingViewModel
                        {
                            Id = 1,
                            TableId = 1,
                            CustomerName = "Alice",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(18).AddMinutes(00),
                            CustomerCount = 2,
                            CustomerPhoneNo = "0215523366",
                            StaffId = 1
                        },
                        new BookingViewModel
                        {
                            Id = 2,
                            TableId = 4,
                            CustomerName = "Bob",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(19).AddMinutes(30),
                            CustomerCount = 4,
                            CustomerPhoneNo = "0248859966",
                            StaffId = 2
                        },
                        new BookingViewModel
                        {
                            Id = 3,
                            TableId = 8,
                            CustomerName = "Sandy",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(20).AddMinutes(00),
                            CustomerCount = 7,
                            CustomerPhoneNo = "0214477553",
                            StaffId = 3
                        }
                    );
                }

                // --- Transactions --- //
                if (!await context.Transactions.AnyAsync())
                {
                    // -- Add Transactions //
                    context.Transactions.AddRange(
                        new Transaction
                        {
                            TransactionId = 1,
                            BookingId = 1,
                            TotalAmount = 50.00m,
                            Status = "Confirmed"
                        },
                        new Transaction
                        {
                            TransactionId = 2,
                            BookingId = 2,
                            TotalAmount = 120.00m,
                            Status = "Pending"
                        },
                        new Transaction
                        {
                            TransactionId = 3,
                            BookingId = 3,
                            TotalAmount = 550.00m,
                            Status = "Pending"
                        }
                    );
                }
                int changes = await context.SaveChangesAsync(); // <----- Save all changes to the database
                Console.WriteLine($"Seeding completed. {changes} changes saved."); // <----- Seeding completed and saved message
            }
        }
    }
}