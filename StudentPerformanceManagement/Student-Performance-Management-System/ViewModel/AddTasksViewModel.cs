using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System.ViewModel
{
    public class AddTasksViewModel
    {
        [Required(ErrorMessage = "Please select course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select staff")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Please select course group")]
        public int CourseGroupId { get; set; }

        [Required(ErrorMessage = "Please select subject")]
        public int SubjectId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> Staffs { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> CourseGroups { get; set; } = new();
    }
}
