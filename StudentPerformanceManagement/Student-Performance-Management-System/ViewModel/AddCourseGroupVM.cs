using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class AddCourseGroupVM
    {
        [Required]
        public string CourseGroupName { get; set; }

        [Required]
        public int CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; }
    }
}
