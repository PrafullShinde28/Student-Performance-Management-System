using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<AppUser> userManager,
                               ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // ENROLL STUDENT (GET)
        [HttpGet]
        public IActionResult EnrollStudent()
        {
            return View();
        }

        // ENROLL STUDENT (POST)
        [HttpPost]
        public async Task<IActionResult> EnrollStudent(string name, string email)
        {
            string defaultPassword = "Student@123";

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");

                var student = new Student
                {
                    Name = name,
                    Email = email,
                    AppUserId = user.Id
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }
    }
}
