using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class AddCourseGroupVM
    {
        [Required(ErrorMessage = "Course group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        public string CourseGroupName { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();
    }
}
