using Microsoft.AspNetCore.Mvc.Rendering;
using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System.ViewModel
{
    public class AddTasksViewModel
    {
        public int CourseId { get; set; }
        public int StaffId { get; set; }
        public string CourseGroupId { get; set; }
       /* public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }*/
        public int SubjectId { get; set; }
        public List<SelectListItem> Courses { get; set; }
        public List<SelectListItem> Staffs { get; set; }
        public List<SelectListItem> Subjects { get; set; }
        public List<SelectListItem> CourseGroups { get; set; }
    }
}
