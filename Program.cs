using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Seeding; 

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);  // <----- Create app builder

        // -- Setup database connection -- //
        builder.Services.AddDbContext<ReservationContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ReservationContext")));

        builder.Services.AddControllersWithViews();

        // -- Configure authentication with cookies -- //
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            });

        var app = builder.Build();

        // -- Ensure database exists with detailed logging -- //
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ReservationContext>();
            Console.WriteLine("Ensuring database exists...");
            var created = context.Database.EnsureCreated();
            Console.WriteLine($"Database ensured: {created}");

            // Debug: Check current data before seeding
            var bookingCount = await context.Bookings.CountAsync();
            var tableCount = await context.Tables.CountAsync();
            var staffCount = await context.Staff.CountAsync();
            Console.WriteLine($"Current data - Bookings: {bookingCount}, Tables: {tableCount}, Staff: {staffCount}");
        }

        // -- Force seed data with detailed logging -- //
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                Console.WriteLine("Force seeding data...");
                await SeedData.Initialize(services);
                Console.WriteLine("Database seeding completed.");

                // Verify what was seeded
                var context = services.GetRequiredService<ReservationContext>();
                var finalBookingCount = await context.Bookings.CountAsync();
                var finalTableCount = await context.Tables.CountAsync();
                var finalStaffCount = await context.Staff.CountAsync();
                Console.WriteLine($"Final data - Bookings: {finalBookingCount}, Tables: {finalTableCount}, Staff: {finalStaffCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        // -- Configure HTTP pipeline -- //
        if (!app.Environment.IsDevelopment())  // <----- Production settings
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // -- Default route -- //
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();  // <----- Start application
    }
}