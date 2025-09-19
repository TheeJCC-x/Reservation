using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservation.Data;
using Reservation.Models;

namespace Reservation.Seeding       //Jene (22501309)
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider) 
        {
            using (var context = new ReservationContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ReservationContext>>()))
            {
                //look for any booking 
                if (context.Tables.Any())
                {
                    return;         //Db has been seeded
                }//end of if
                context.Tables.AddRange(
                        new TableViewModel
                        {
                            Id = 1,
                            Availability = true,
                            Seats = 2,
                            TableNumber = 1
                        },
                        new TableViewModel
                        {
                            Id = 2,
                            Availability = true,
                            Seats = 2,
                            TableNumber = 2
                        },
                        new TableViewModel
                        {
                            Id = 3,
                            Availability = true,
                            Seats = 4,
                            TableNumber = 3
                        },
                        new TableViewModel
                        {
                            Id = 4,
                            Availability = true,
                            Seats = 4,
                            TableNumber = 4
                        },
                        new TableViewModel
                        {
                            Id = 5,
                            Availability = true,
                            Seats = 6,
                            TableNumber = 5
                        },
                        new TableViewModel
                        {
                            Id = 6,
                            Availability = true,
                            Seats = 6,
                            TableNumber = 6
                        },
                        new TableViewModel
                        {
                            Id = 7,
                            Availability = true,
                            Seats = 8,
                            TableNumber = 7
                        },
                        new TableViewModel
                        {
                            Id = 8,
                            Availability = true,
                            Seats = 8,
                            TableNumber = 8
                        }
                );

                // --- Seed Staff ---
                if (context.Staff.Any())
                {
                    return;
                }
                context.Staff.AddRange(
                        new Staff
                        {
                            StaffId = 1,
                            StaffName = "Jiteesh",
                        },
                        new Staff
                        {
                            StaffId = 2,
                            StaffName = "Byron",
                        },
                        new Staff
                        {
                            StaffId = 3,
                            StaffName = "Jene",
                        }
                       );

                // --- Seed Bookings ---
                if (context.Bookings.Any())
                {
                    return;
                }

                context.Bookings.AddRange(
                        new BookingViewModel 
                        {   Id = 1, 
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
                            Id = 2,
                            TableId = 8,
                            CustomerName = "Sandy",
                            BookingDate = DateTime.Today,
                            BookingTime = DateTime.Today.AddHours(20).AddMinutes(00),
                            CustomerCount = 7,
                            CustomerPhoneNo = "0214477553",
                            StaffId = 3
                        }
                    );

                // --- Seed Transactions ---
                if (context.Transactions.Any())
                {
                    return;
                }
                context.Transactions.AddRange(
                        new Transaction 
                        {
                            TransactionId = 1, 
                            BookingId = 1,
                            TotalAmount = 50.00m,  
                            Status = "Confirmed" },
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

                context.SaveChanges();

            }//end of using

        }//end of initialize
        
                        
    }//end of class
}//end 
