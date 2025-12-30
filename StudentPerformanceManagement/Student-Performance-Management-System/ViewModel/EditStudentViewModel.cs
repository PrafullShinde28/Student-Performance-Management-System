using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Student_Performance_Management_System.ViewModel
{
    public class EditStudentViewModel
    {
        public int StudentId { get; set; }
        public string AppUserId { get; set; }

        public string PRN { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        //  Gmail validation
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$",
            ErrorMessage = "Email must end with @gmail.com")]
        public string Email { get; set; }

        //  10-digit mobile validation
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select a course group")]
        public int CourseGroupId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> CourseGroups { get; set; } = new();
    }
}
