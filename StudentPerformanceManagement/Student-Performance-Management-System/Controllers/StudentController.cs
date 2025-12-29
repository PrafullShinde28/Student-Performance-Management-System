using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Performance_Management_System.Models;
using Student_Performance_Management_System.ViewModel;
using StudentPerformanceManagment.ViewModel;
using System.Security.Claims;



namespace Student_Performance_Management_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {

        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public StudentController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        private async Task<StudentViewModel> GetData()
        {
            var userId = _userManager.GetUserId(User);

            // 1. Single Query with Joins: Student, Course, aur Group ko ek saath fetch karein
            var student = await _context.Students
                .Include(s => s.Course)
                .Include(s => s.CourseGroup)
                .Where(s => s.AppUserId == userId)
                .FirstOrDefaultAsync();

            if (student == null) return new StudentViewModel();

            // 2. Optimized Count: Subject count ke liye alag query
            int subjectCount = _context.Subjects.Where(s => s.CourseId == student.CourseId).Count();
            int rank = GetStudentRank(student.StudentId, student.CourseId);

            // 3. Mapping to ViewModel
            var stud = new StudentViewModel()
            {
                StudentId = student.StudentId,
                PRN = student.PRN,
                Name = student.Name,
                Email = User.Identity?.Name, // Identity se email lena fast hai
                CourseName = student.Course?.CourseName ?? "N/A",
                SubjectCount = subjectCount,
                CourseGroupName = student.CourseGroup?.GroupName ?? "N/A",
                MobileNo = student.MobileNo,
                Rank = rank,

            };

            return stud;
        }


        public async Task<IActionResult> Dashboard()
        {

            var stud = await GetData();
            return View(stud);

        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var stud = await GetData();
            return View(stud);
        }

        [HttpPost]
        public async Task<IActionResult> AfterEditProfile(StudentViewModel model)
        {


            var userId = _userManager.GetUserId(User);
            var appUser = await _userManager.FindByIdAsync(userId);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.AppUserId == userId);

            if (student == null) return NotFound();

            // 2. Profile Data Update (Name & Mobile)
            student.Name = model.Name;
            student.MobileNo = model.MobileNo;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();



            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard");
        }



        public IActionResult ChangePassword()
        {

            return View(new PasswordViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(PasswordViewModel model)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");


            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password updated successfully!";
                return RedirectToAction("Dashboard", "Student");
            }

            foreach (var error in result.Errors)
            {

                if (error.Code.Contains("PasswordMismatch"))
                {
                    ModelState.AddModelError("CurrentPassword", "The current password you entered is incorrect.");
                }
                else
                {

                    ModelState.AddModelError("NewPassword", error.Description);
                }
            }

            return View("ChangePassword", model);
        }

        public int GetStudentRank(int studentId, int courseId)
        {
            var students = _context.Students.Where(s => s.CourseId == courseId).Include(s => s.Marks).ToList();
            var markList = students.Select(s => new StudentMarks
            {
                StudentId = s.StudentId,
                TotalMarks = s.Marks.Sum(m => m.TotalMarks)
            }
            ).OrderByDescending(sm => sm.TotalMarks);

            int rank = 0;
            int prevMarks = -1;

            foreach (var item in markList)
            {
                if (item.TotalMarks != prevMarks)
                {
                    rank++;
                    prevMarks = item.TotalMarks;
                }

                if (item.StudentId == studentId)
                    return rank;
            }
            return 0;// student not found
        }

        public IActionResult ViewPerformanceCard(int id)
        {
            var student = _context.Students
                .Include(s => s.Course)
                .Include(s => s.CourseGroup)
                .Include(s => s.Marks)
                .ThenInclude(m => m.Subject)
                .FirstOrDefault(s => s.StudentId == id);
            /*var subjects = _db.Students
                .Include(s => s.Marks)
                    .ThenInclude(m => m.Subject).ToList();*/
            int rank = GetStudentRank(id, student.CourseId);
            if (student == null)
                return NotFound();

            var vm = new PerformanceCard
            {
                Rank = rank,
                StudentPRN = student.PRN,
                StudentName = student.Name,
                CourseName = student.Course.CourseName,
                Subjects = student.Marks.Select(m => new SubjectMarksViewModel
                {
                    SubjectName = m.Subject.SubjectName,
                    Theory = m.TheoryMarks,
                    Lab = m.LabMarks,
                    Internal = m.InternalMarks,
                    Total = m.TotalMarks,
                    Status = m.IsPass(),
                    MaxMarks = m.Subject.MaxLabMarks + m.Subject.MaxLabMarks + m.Subject.MaxLabMarks,
                    FailedIn = m.FailedIn(),
                }).ToList()
            };

            return View(vm);
        }
    }
}
