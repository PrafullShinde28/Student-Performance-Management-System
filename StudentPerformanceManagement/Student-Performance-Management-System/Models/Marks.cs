namespace Student_Performance_Management_System.Models
{
    public class Marks
    {
        public int MarksId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null;

        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = null;

        public int TheoryMarks { get; set; }
        public int LabMarks { get; set; }

        public int InternalMarks { get; set; }

        public int TotalMarks => TheoryMarks + LabMarks + InternalMarks;
    }
}
