namespace Student_Performance_Management_System.ViewModel
{
    public class EditStudentVM
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public IFormFile? Profile { get; set; }
       
    }
}
