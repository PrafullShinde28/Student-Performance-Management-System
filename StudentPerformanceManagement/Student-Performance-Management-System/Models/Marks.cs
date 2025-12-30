namespace Student_Performance_Management_System.Models
{
    public class Marks
    {
        public int MarksId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null;

        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = null;

        public int TasksId {  get; set; }
        public Tasks Tasks {  get; set; }

        public int TheoryMarks { get; set; }
        public int LabMarks { get; set; }

        public int InternalMarks { get; set; }

        public int TotalMarks => TheoryMarks + LabMarks + InternalMarks;

        public bool IsPass()
        {
            int mt = Subject.MaxTheoryMarks;
            int mi = Subject.MaxInternalMarks;
            int ml = Subject.MaxLabMarks;
            int passingPercent = Subject.PassingPercentEachComponent;
            /*(current / maximum) * 100*/

            double theoryPercent = (TheoryMarks / (Double)mt) * 100;
            double labPercent = (InternalMarks / (Double)ml) * 100;
            double internalPercent = (LabMarks / (Double)mi) * 100;

            if ((theoryPercent >= passingPercent && labPercent >= passingPercent)
                || (labPercent >= passingPercent && internalPercent >= passingPercent)
                || (internalPercent >= passingPercent && theoryPercent >= passingPercent)
                ) return true;
            else return false;
        }

        public string FailedIn()
        {
            int mt = Subject.MaxTheoryMarks;
            int mi = Subject.MaxInternalMarks;
            int ml = Subject.MaxLabMarks;
            int passingPercent = Subject.PassingPercentEachComponent;
            /*(current / maximum) * 100*/

            double theoryPercent = (TheoryMarks / (Double)mt) * 100;
            double labPercent = (InternalMarks / (Double)ml) * 100;
            double internalPercent = (LabMarks / (Double)mi) * 100;

            if (theoryPercent < passingPercent && labPercent < passingPercent && internalPercent < passingPercent) return "F";
            else if (theoryPercent < passingPercent && labPercent < passingPercent) return "TL";
            else if (theoryPercent < passingPercent && internalPercent < passingPercent) return "TI";
            else if (internalPercent < passingPercent && labPercent < passingPercent) return "IL";
            else if (internalPercent < passingPercent) return "I";
            else if (theoryPercent < passingPercent) return "T";
            else if (labPercent < passingPercent) return "L";
            return "P";
        }
    }
}
