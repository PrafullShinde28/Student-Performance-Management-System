using Microsoft.AspNetCore.Mvc.Rendering;
using Student_Performance_Management_System.Models;

public class TaskFilterVM
{
    public int? StaffId { get; set; }
    public Status? Status { get; set; }   

    public List<SelectListItem> StaffList { get; set; }
    public List<SelectListItem> StatusList { get; set; }

    public List<Tasks> Tasks { get; set; }
}
