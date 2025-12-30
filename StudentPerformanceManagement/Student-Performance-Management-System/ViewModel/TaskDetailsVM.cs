using System;

namespace Student_Performance_Management_System.ViewModel
{
    public class TaskDetailsVM
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public string CourseName { get; set; }
        public string CourseGroupName { get; set; }
        public string SubjectName { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }

        public string Status { get; set; }
    }
}
