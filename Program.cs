using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Seeding;

public class Program
{
    public static async Task Main(string[] args)  // <----- Application entry point
    {
        var builder = WebApplication.CreateBuilder(args);  // <----- Create app builder

        // -- Setup database connection -- //
        builder.Services.AddDbContext<ReservationContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ReservationContext")));

        builder.Services.AddControllersWithViews();  // <----- Add MVC support

        // -- Configure authentication with cookies -- //
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";  // <----- Login page
                options.AccessDeniedPath = "/Account/AccessDenied";  // <----- Access denied page
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // <----- Cookie expiration
            });

        var app = builder.Build();  // <----- Build application

        // -- Ensure database exists -- //
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ReservationContext>();
            context.Database.EnsureCreated();  // <----- Create database if missing
        }

        // -- Seed database with sample data -- //
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                Console.WriteLine("Starting database seeding...");  // <----- Start seeding
                await SeedData.Initialize(services);  // <----- Run seed data
                Console.WriteLine("Database seeding completed.");  // <----- Completion message
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding error: {ex.Message}");  // <----- Error handling
            }
        }

        // -- Configure HTTP pipeline -- //
        if (!app.Environment.IsDevelopment())  // <----- Production settings
        {
            app.UseExceptionHandler("/Home/Error");  // <----- Error page
            app.UseHsts();  // <----- HTTPS enforcement
        }

        app.UseHttpsRedirection();  // <----- HTTPS redirect
        app.UseStaticFiles();  // <----- Serve static files
        app.UseRouting();  // <----- Enable routing
        app.UseAuthentication();  // <----- Enable authentication
        app.UseAuthorization();  // <----- Enable authorization

        // -- Default route -- //
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();  // <----- Start application
    }
}