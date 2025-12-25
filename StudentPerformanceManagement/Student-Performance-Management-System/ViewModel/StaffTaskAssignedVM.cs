using System.Collections.Generic;

namespace Student_Performance_Management_System.ViewModel
{
    public class StaffTaskAssignedVM
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }

        public List<TaskDetailsVM> Tasks { get; set; } = new();
    }
}
