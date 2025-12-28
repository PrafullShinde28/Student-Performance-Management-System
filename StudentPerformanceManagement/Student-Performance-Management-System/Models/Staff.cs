using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Student_Performance_Management_System.Models
{
    public class Staff
    {
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string MobileNo { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
    }
}
