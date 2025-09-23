using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservation.Data;
using Reservation.Models;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// Updated by Byron (21/09/2025)
// Updated by Byron (22/09/2025) -- *Removed Specific ID's from tables due to ID Auto-Increments (Fixing Database Table Issue)
// Updated by Jene (24/09/2025) -- Removed the Admin account, Customer and Transaction seed data.

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

                if (!await context.Account.AnyAsync(a => a.Username == "staff")) // <------ Check if staff exists
                {
                    // -- Add Staff Account //
                    context.Account.Add(new Account
                    {
                        Username = "staff",
                        Password = BCrypt.Net.BCrypt.HashPassword("password1"), // <------ Hashed Staff Password
                        RememberMe = false
                    });
                    Console.WriteLine("Staff account created"); // <----- Staff Account Successfully Created
                    await context.SaveChangesAsync();
                }                

                // --- Tables --- //
                // Updated 22/09/2025: Removed Specific ID's due to Auto Incrementing for ID's
                if (!await context.Tables.AnyAsync()) // <------ Check if the table exists
                {
                    // -- Add Tables //
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

                Console.WriteLine("Seeding completed successfully"); // <----- Seeding Completed Confirmation Message
            } 
        }
    }
}