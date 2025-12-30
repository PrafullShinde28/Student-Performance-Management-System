namespace Student_Performance_Management_System.ViewModel
{
    public class AddTask
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int CourseId { get; set; }
        public int CourseGroupId { get; set; }
        public int SubjectId { get; set; }
        public int StaffId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
