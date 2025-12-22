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

            builder.Entity<Student>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // or NoAction

            builder.Entity<Student>()
                .HasOne(s => s.CourseGroup)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.CourseGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tasks>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tasks>()
               .HasOne(t => t.Subject)
               .WithMany()
               .HasForeignKey(t => t.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tasks>()
               .HasOne(t => t.CourseGroup)
               .WithMany()
               .HasForeignKey(t => t.CourseGroupId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tasks>()
               .HasOne(t => t.Staff)
               .WithMany()
               .HasForeignKey(t => t.StaffId)
               .OnDelete(DeleteBehavior.Restrict);

        }


    }
}
