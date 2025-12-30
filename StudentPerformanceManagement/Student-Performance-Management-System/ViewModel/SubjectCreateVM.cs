using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class SubjectCreateVM
    {
        [Required(ErrorMessage = "Subject name is required")]
        public string SubjectName { get; set; }

        [Required(ErrorMessage = "Please select course")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select course")]
        public int CourseId { get; set; }

        [BindNever] // 👈 THIS STOPS MVC FROM VALIDATING CourseList
        public IEnumerable<SelectListItem> CourseList { get; set; }
    }
}
