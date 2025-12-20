namespace Student_Performance_Management_System.Models
{
    using Microsoft.AspNetCore.Identity;

    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
