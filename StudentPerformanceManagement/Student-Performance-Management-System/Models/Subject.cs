using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null;

        public int MaxTheoryMarks { get; set; } = 40;

        public int MaxLabMarks { get; set; } = 40;

        public int MaxInternalMarks { get; set; } = 20;

        public int PassingPercentTotal { get; set; } = 40;

        public int PassingPercentEachComponent { get; set; } = 40;

        public Marks Marks { get; set; }
    }
}
