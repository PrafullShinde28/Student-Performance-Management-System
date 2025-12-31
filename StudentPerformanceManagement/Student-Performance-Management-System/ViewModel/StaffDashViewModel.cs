using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System.ViewModel
{
    public class StaffDashViewModel
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public int TaskCount { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string? Profile { get; set; }
        // yeh tumhari Task entity ka type hoga (jo _context.Tasks se aata hai)
        public List<Tasks> Tasks { get; set; } = new List<Tasks>();
    }
}