using System.ComponentModel.DataAnnotations;

namespace Student_Performance_Management_System.ViewModel
{
    public class EditTaskVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select course")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select course group")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select course group")]
        public int CourseGroupId { get; set; }

        [Required(ErrorMessage = "Please select subject")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select subject")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Please select staff")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select staff")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Valid From date is required")]
        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid To date is required")]
        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; }
    }
}
