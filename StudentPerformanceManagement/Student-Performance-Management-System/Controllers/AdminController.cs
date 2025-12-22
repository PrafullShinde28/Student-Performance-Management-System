using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Student_Performance_Management_System.Models;

namespace Student_Performance_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _db;


        public AdminController(UserManager<AppUser> userManager,
                               ApplicationDbContext context, ApplicationDbContext db)
        {
            _userManager = userManager;
            _context = context;
            _db = db;
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


        public string GeneratePRN()
        {
            //int year = DateTime.Now.Year;
            int year = 2026;
            string basePart = year + "1000";

            var lastPRN = _db.Students
                            .OrderByDescending(s => s.PRN)
                            .Select(s => s.PRN)
                            .FirstOrDefault();

            if (lastPRN == null)
            {
                return basePart + "0001";
            }
            else
            {
                string last = lastPRN.Substring(basePart.Length);
                int next = int.Parse(last) + 1;

                return basePart + next.ToString("D4");
            }
        }


        // ENROLL STUDENT (POST)
        [HttpPost]
        public async Task<IActionResult> EnrollStudent(string name, string email, string mobileno, int courseid, int coursegroupid)
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
                    PRN = GeneratePRN(),
                    ProfileImagePath = "default.png",
                    Name = name,
                    Email = email,
                    AppUserId = user.Id,
                    MobileNo = mobileno,
                    CourseId = courseid,
                    CourseGroupId = coursegroupid,


                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }
        // ADD STAFF (GET)
        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }

        // ADD STAFF (POST)
        [HttpPost]
        public async Task<IActionResult> AddStaff(string name, string email, string mno)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true
            };
            string pass = "Staff@123";

            var result = await _userManager.CreateAsync(user, pass);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Staff");

                var staff = new Staff
                {
                    Name = name,
                    Email = email,
                    MobileNo = mno,
                    Password = pass,
                    AppUserId = user.Id
                };

                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        }
}
