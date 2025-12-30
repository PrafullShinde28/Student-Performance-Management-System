using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class StudentEnrollmentViewModel
    {
        // ✅ NAME REQUIRED
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        // ✅ EMAIL REQUIRED + GMAIL ONLY
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$",
            ErrorMessage = "Email must end with @gmail.com")]
        public string Email { get; set; }

        // ✅ MOBILE REQUIRED + EXACTLY 10 DIGITS
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string MobileNo { get; set; }

        // ✅ COURSE REQUIRED
        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        // ✅ COURSE GROUP REQUIRED
        [Required(ErrorMessage = "Please select a course group")]
        public int CourseGroupId { get; set; }

        // Dropdown Data
        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> CourseGroups { get; set; } = new();

        
        public string? ProfileImagePath { get; set; } = "profile.img";
    }
}
