using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Student_Performance_Management_System.Models;
using System.Threading.Tasks;

namespace Student_Performance_Management_System
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseGroup> CourseGroups { get; set; }

        public DbSet<Tasks> Tasks { get; set; }


        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Marks> Marks { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
        }


    }
}
