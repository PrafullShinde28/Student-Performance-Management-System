using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class AddTask
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(300)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select course group")]
        public int CourseGroupId { get; set; }

        [Required(ErrorMessage = "Please select subject")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Please select staff")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Valid from date is required")]
        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid to date is required")]
        [DataType(DataType.Date)]
        [DateGreaterThan("ValidFrom", ErrorMessage = "Valid To must be after Valid From")]
        public DateTime ValidTo { get; set; }
    }
}
