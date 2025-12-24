using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string PRN { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }

        [MaxLength(10)]
        public string? MobileNo { get; set; }

        // Store the only in db and actually image store in wwwroot
        public string ProfileImagePath {  get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int CourseGroupId { get; set; }
        public CourseGroup CourseGroup { get; set; }

        public ICollection<Marks> Marks { get; set; } = new List<Marks>();
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
