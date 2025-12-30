using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=SPMSDB;Trusted_Connection=True;"));

            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var app = builder.Build();

            // Seed
            using (var scope = app.Services.CreateScope())
            {
                await IdentitySeed.SeedRolesAndAdmin(scope.ServiceProvider);
            }

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // <--- Add this
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
