using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservation.Data;
using Reservation.Models;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// Updated by Byron (21/09/2025)
// Updated by Byron (22/09/2025) -- *Removed Specific ID's from tables due to ID Auto-Increments (Fixing Database Table Issue)

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
                    context.Account.Add(new Account
                    {
                        Username = "admin",
                        Password = adminPasswordHashed, // <------ Hashed Admin Password
                        RememberMe = false
                    });
                    Console.WriteLine("Admin account created"); // <----- Admin Account Successfully Created
                }

                if (!await context.Account.AnyAsync(a => a.Username == "staff")) // <------ Check if staff exists
                {
                    string staffPasswordHashed = BCrypt.Net.BCrypt.HashPassword("password1"); // <------ Hash the staff password
                    context.Account.Add(new Account
                    {
                        Username = "staff",
                        Password = staffPasswordHashed, // <------ Hashed Staff Password
                        RememberMe = false
                    });
                    Console.WriteLine("Staff account created"); // <----- Staff Account Successfully Created
                }
                await context.SaveChangesAsync();

                // --- Tables --- //
                // Updated 22/09/2025: Removed Specific ID's due to Auto Incrementing for ID's
                if (!await context.Tables.AnyAsync())
                {
                    Console.WriteLine("Seeding tables...");
                    context.Tables.AddRange(
                        new TableViewModel { Availability = true, Seats = 2, TableNumber = 1 },
                        new TableViewModel { Availability = true, Seats = 2, TableNumber = 2 },
                        new TableViewModel { Availability = true, Seats = 4, TableNumber = 3 },
                        new TableViewModel { Availability = true, Seats = 4, TableNumber = 4 },
                        new TableViewModel { Availability = true, Seats = 6, TableNumber = 5 },
                        new TableViewModel { Availability = true, Seats = 6, TableNumber = 6 },
                        new TableViewModel { Availability = true, Seats = 8, TableNumber = 7 },
                        new TableViewModel { Availability = true, Seats = 8, TableNumber = 8 }
                    );
                    await context.SaveChangesAsync();
                    Console.WriteLine("Tables seeded");
                }

                // --- Staff --- //
                // Updated 22/09/2025: Removed Specific ID's due to Auto Incrementing for ID's
                if (!await context.Staff.AnyAsync())
                {
                    Console.WriteLine("Seeding staff...");
                    context.Staff.AddRange(
                        new Staff { StaffName = "Jiteesh" },
                        new Staff { StaffName = "Byron" },
                        new Staff { StaffName = "Jene" }
                    );
                    await context.SaveChangesAsync();
                    Console.WriteLine("Staff seeded");
                }

                // --- Seed only seeded bookings --- //
                // --- Check if seeded bookings already exist --- //
                bool sampleBooking1Exists = await context.Bookings.AnyAsync(b =>
                    b.CustomerName == "Alice" && b.CustomerPhoneNo == "0215523366");
                bool sampleBooking2Exists = await context.Bookings.AnyAsync(b =>
                    b.CustomerName == "Bob" && b.CustomerPhoneNo == "0248859966");
                bool sampleBooking3Exists = await context.Bookings.AnyAsync(b =>
                    b.CustomerName == "Sandy" && b.CustomerPhoneNo == "0214477553");

                if (!sampleBooking1Exists || !sampleBooking2Exists || !sampleBooking3Exists)
                {
                    Console.WriteLine("Seeding sample bookings..."); // <----- Seeding the specific data from this file

                    // Retrieve references for the seeded data
                    var table1 = await context.Tables.FirstOrDefaultAsync(t => t.TableNumber == 1); // <----- Find Table 1 in the database
                    var table4 = await context.Tables.FirstOrDefaultAsync(t => t.TableNumber == 4); // <----- Find Table 4 in the database
                    var table8 = await context.Tables.FirstOrDefaultAsync(t => t.TableNumber == 8); // <----- Find Table 8 in the database

                    var staff1 = await context.Staff.FirstOrDefaultAsync(s => s.StaffName == "Jiteesh"); // <----- Find Staff member in the database
                    var staff2 = await context.Staff.FirstOrDefaultAsync(s => s.StaffName == "Byron"); // <----- Find Staff member in the database
                    var staff3 = await context.Staff.FirstOrDefaultAsync(s => s.StaffName == "Jene"); // <----- Find Staff member in the database

                    if (table1 != null && staff1 != null && !sampleBooking1Exists) // <----- Check if sample booking exists
                    {
                        context.Bookings.Add(new BookingViewModel
                        // Create a new booking for Alice at Table 1 with Staff Jiteesh
                        {
                            TableId = table1.Id,
                            CustomerName = "Alice",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(18).AddMinutes(00),
                            CustomerCount = 2,
                            CustomerPhoneNo = "0215523366",
                            StaffId = staff1.StaffId
                        });
                    }

                    if (table4 != null && staff2 != null && !sampleBooking2Exists) // <----- Check if sample booking exists
                    {
                        context.Bookings.Add(new BookingViewModel
                        // Create a new booking for Bob at Table 4 with Staff Byron
                        {
                            TableId = table4.Id,
                            CustomerName = "Bob",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(19).AddMinutes(30),
                            CustomerCount = 4,
                            CustomerPhoneNo = "0248859966",
                            StaffId = staff2.StaffId
                        });
                    }

                    if (table8 != null && staff3 != null && !sampleBooking3Exists) // <----- Check if sample booking exists
                    {
                        context.Bookings.Add(new BookingViewModel
                        // Create a new booking for Sandy at Table 8 with Staff Jene
                        {
                            TableId = table8.Id,
                            CustomerName = "Sandy",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(20).AddMinutes(00),
                            CustomerCount = 7,
                            CustomerPhoneNo = "0214477553",
                            StaffId = staff3.StaffId
                        });
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine("Sample bookings seeded");
                }

                // --- Sample Transactions (for sample bookings only) --- //
                if (!await context.Transactions.AnyAsync())
                {
                    Console.WriteLine("Seeding sample transactions...");

                    // Find our sample bookings
                    var aliceBooking = await context.Bookings
                        .FirstOrDefaultAsync(b => b.CustomerName == "Alice" && b.CustomerPhoneNo == "0215523366");
                    var bobBooking = await context.Bookings
                        .FirstOrDefaultAsync(b => b.CustomerName == "Bob" && b.CustomerPhoneNo == "0248859966");
                    var sandyBooking = await context.Bookings
                        .FirstOrDefaultAsync(b => b.CustomerName == "Sandy" && b.CustomerPhoneNo == "0214477553");

                    if (aliceBooking != null) // <----- Check if booking transaction exists
                    {
                        context.Transactions.Add(new Transaction
                        {
                            BookingId = aliceBooking.Id,
                            TotalAmount = 50.00m,
                            Status = "Confirmed"
                        });
                    }

                    if (bobBooking != null) // <-----Check if booking transaction exists
                    {
                        context.Transactions.Add(new Transaction
                        {
                            BookingId = bobBooking.Id,
                            TotalAmount = 120.00m,
                            Status = "Pending"
                        });
                    }

                    if (sandyBooking != null) // <-----Check if booking transaction exists
                    {
                        context.Transactions.Add(new Transaction
                        {
                            BookingId = sandyBooking.Id,
                            TotalAmount = 550.00m,
                            Status = "Pending"
                        });
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine("Sample transactions seeded");
                }

                Console.WriteLine("Seeding completed successfully");
            }
        }
    }
}