namespace Student_Performance_Management_System.Models
{
    public class CourseGroup
    {
        public int CourseGroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int GroupSize { get; set; } = 10;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
