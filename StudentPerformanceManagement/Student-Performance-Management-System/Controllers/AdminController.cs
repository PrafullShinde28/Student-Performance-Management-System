using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Student_Performance_Management_System.Models;
using Student_Performance_Management_System.ViewModel;

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
        public IActionResult Students()
        {
            var students = _context.Students.ToList();
            return View(students);
        }
        public IActionResult Courses()
        {
            return View();
        }


        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }


        [HttpPost]
        public async Task<IActionResult> EditStudent(Student model)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentId == model.StudentId);

            if (student == null)
                return NotFound();


            student.Name = model.Name;
            student.Email = model.Email;
            student.MobileNo = model.MobileNo;
            student.CourseId = model.CourseId;
            student.CourseGroupId = model.CourseGroupId;


            var user = await _userManager.FindByIdAsync(student.AppUserId);
            if (user != null)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                await _userManager.UpdateAsync(user);
            }

            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction("Students");
        }


        // DELETE STUDENT (POST)
        [HttpPost]

        public async Task<IActionResult> DeleteStudent(int id)
        {

            var student = _context.Students.FirstOrDefault(s => s.StudentId == id);
            if (student == null)
                return NotFound();


            var user = await _userManager.FindByIdAsync(student.AppUserId);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction("Students");
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

        [HttpGet]
        public IActionResult EnrollStudent()
        {
            var model = new StudentEnrollmentViewModel
            {
                Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Text = c.CourseName.ToString(),
                        Value = c.CourseId.ToString(),
                    }).ToList(),

                CourseGroups = _context.CourseGroups
                    .Select(g => new SelectListItem
                    {
                        Text = g.GroupName.ToString(),
                        Value = g.CourseGroupId.ToString()
                    }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EnrollStudent(StudentEnrollmentViewModel model,
                                                 StudentEnrollmentViewModel1 model1)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Text = c.CourseName,
                    Value = c.CourseId.ToString()
                }).ToList();

                model.CourseGroups = _context.CourseGroups.Select(g => new SelectListItem
                {
                    Text = g.GroupName,
                    Value = g.CourseGroupId.ToString()
                }).ToList();

                return View(model);
            }

            string defaultPassword = "Student@123";

            var user = new AppUser
            {
                UserName = model1.Email,
                Email = model1.Email,
                FullName = model1.Name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");

                var student = new Student
                {
                    PRN = GeneratePRN(),
                    Name = model1.Name,
                    Email = model1.Email,
                    AppUserId = user.Id,
                    MobileNo = model1.MobileNo,
                    CourseId = model.CourseId,
                    CourseGroupId = model.CourseGroupId,
                    ProfileImagePath = model1.ProfileImagePath
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Account");
            }

            return View(model);
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
