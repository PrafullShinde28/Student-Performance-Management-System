using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Performance_Management_System.Models;
using Student_Performance_Management_System.ViewModel;

namespace Student_Performance_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        public AccountController(SignInManager<AppUser> signInManager,
                                 UserManager<AppUser> userManager, ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
        }

        // LOGIN PAGE
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(
                email, password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // ROLE BASED DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var stats = new DashboardViewModel
                    {
                        TotalCourses = _db.Courses.Count(),
                        TotalGroups = _db.CourseGroups.Count(),
                        TotalTasks = _db.Tasks.Count(),
                        TotalStudent = _db.Students.Count(),
                        TotalStaff = _db.Staffs.Count(),
                        TotalSubjects = _db.Subjects.Count()
                    };
                    return View("AdminDashboard", stats);
                }
            }

            if (await _userManager.IsInRoleAsync(user, "Staff"))
                return RedirectToAction("Dashboard", "Staff");

            return RedirectToAction("Dashboard", "Student");
        }

        // LOGOUT (POST ONLY)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
