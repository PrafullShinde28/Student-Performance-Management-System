using System.Threading.Tasks;

namespace Student_Performance_Management_System.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public ICollection<Tasks> Tasks = new List<Tasks>();
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
