namespace Student_Performance_Management_System.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public string Description { get; set; }
        public int Duration { get; set; }
        public decimal Fees { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<CourseGroup> Groups { get; set; } = new List<CourseGroup>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
