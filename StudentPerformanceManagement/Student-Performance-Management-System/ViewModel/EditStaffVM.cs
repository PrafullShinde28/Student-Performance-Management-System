namespace Student_Performance_Management_System.ViewModel
{
    public class EditStaffVM
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
