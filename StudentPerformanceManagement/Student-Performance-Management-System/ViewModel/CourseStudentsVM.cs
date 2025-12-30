namespace Student_Performance_Management_System.ViewModel
{
    public class CourseStudentsVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public List<CourseStudentItemVM> Students { get; set; } = new();
    }

    public class CourseStudentItemVM
    {
        public string PRN { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CourseGroupName { get; set; }
        public string MobileNo { get; set; }
    }
}
