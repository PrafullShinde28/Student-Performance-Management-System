using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Student_Performance_Management_System.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(10)]
        public string MobileNo { get; set; }
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
