using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Student_Performance_Management_System.Models;
using Student_Performance_Management_System.ViewModel;

using System.Security.Claims;

namespace Student_Performance_Management_System.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public StaffController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
       

        // CHANGE PASSWORD (GET)
        public IActionResult ChangePassword()
        {
            return View(new StaffChangePasswordViewModel());
        }

        // CHANGE PASSWORD (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(StaffChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ChangePassword", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password updated successfully!";
                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
            {
                if (error.Code.Contains("PasswordMismatch"))
                {
                    ModelState.AddModelError(
                        "CurrentPassword",
                        "The current password you entered is incorrect."
                    );
                }
                else
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        error.Description
                    );
                }
            }

            return View("ChangePassword", model);
        }



        public async Task<IActionResult> MyTasks()
        {
            UpdateOverdueTasks();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var staff = await _context.Staffs
                        .FirstOrDefaultAsync(s => s.AppUserId == userId);
            var myTasks = await _context.Tasks
                .Include(t => t.Course)
                .Include(t => t.Subject)
                .Include(t => t.CourseGroup)
                .Where(t => t.Staff.AppUserId == userId)
                .ToListAsync();

            var vm = new StaffDashViewModel
            {

                // StaffDashViewModel properties
                StaffId = staff.StaffId,
                StaffName = staff.Name,
                TaskCount = myTasks.Count,
                Tasks = myTasks,
                Profile = staff.ProfileImage,
                Email = staff.Email,
                MobileNo = staff.MobileNo
            };

            return View("MyTasks", vm);  // ya sirf return View(vm);
        }
        public async Task<IActionResult> Dashboard()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var staff = await _context.Staffs
                        .FirstOrDefaultAsync(s => s.AppUserId == userId);
            var myTasks = await _context.Tasks
                .Include(t => t.Course)
                .Include(t => t.Subject)
                .Include(t => t.CourseGroup)
                .Where(t => t.Staff.AppUserId == userId)
                .ToListAsync();

            var vm = new StaffDashViewModel
            {
                // StaffDashViewModel properties
                StaffId = staff.StaffId,
                StaffName = staff.Name,
                TaskCount = myTasks.Count,
                Tasks = myTasks,
                Profile = staff.ProfileImage,
                Email = staff.Email,
                MobileNo = staff.MobileNo
            };
            return View("Dashboard", vm);   // YAHAN StaffDashViewModel hi return karo
        }

        public IActionResult AddMark(int id)

        {

            // id = 3;
            var task = _context.Tasks.Include(c => c.Course)
                .Include(cg => cg.CourseGroup)
                .Include(s => s.Subject)
                .Where(t => t.TasksId == id).FirstOrDefault();


            var students = _context.Students.Where(s => s.CourseGroupId == task.CourseGroupId)
                .Select(s => new MarkViewModel
                {

                    StudentId = s.StudentId,
                    SubjectId = task.SubjectId,
                    CourseGroupId = task.CourseGroupId,
                    // CourseId = task.CourseId,
                    PRN = s.PRN,
                    Name = s.Name,
                    TaskId = task.TasksId,
                    TheoryMarks = _context.Marks.Where(m => m.TasksId == task.TasksId && m.StudentId == s.StudentId)
                                    .Select(m => m.TheoryMarks).FirstOrDefault(),


                    LabMarks = _context.Marks.Where(m => m.TasksId == task.TasksId && m.StudentId == s.StudentId)
                                    .Select(m => m.LabMarks).FirstOrDefault(),

                    InternalMarks = _context.Marks.Where(m => m.TasksId == task.TasksId && m.StudentId == s.StudentId)
                                    .Select(m => m.InternalMarks).FirstOrDefault(),

                }).ToList();

            UpdateStudentViewModel.markcount = _context.Marks.Where(m => m.TasksId == task.TasksId).Count();
            UpdateStudentViewModel.studcount = students.Count();


            return View(students);
        }
        private void UpdateOverdueTasks()
        {
            var now = DateTime.Now;

            var tasks = _context.Tasks.ToList();

            foreach (var t in tasks)
            {
                // 1. Pending → Overdue
                if (t.Status == Status.Pending && now > t.ValidTo)
                {
                    t.Status = Status.Overdue;
                }

                // 2. LateApproved → Pending (after extension)
                else if (t.Status == Status.LateApproved && now <= t.ValidTo)
                {
                    t.Status = Status.Pending;
                }

                // 3. LateApproved → Overdue (if again missed)
                else if (t.Status == Status.LateApproved && now > t.ValidTo)
                {
                    t.Status = Status.Overdue;
                }
            }

            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveMark(UpdateStudentViewModel markviewmodel)
        {
            var subject = _context.Subjects.Find(markviewmodel.SubjectId);

            var existingMark = _context.Marks
                .FirstOrDefault(m => m.StudentId == markviewmodel.StudentId && m.TasksId == markviewmodel.TaskId);



            if (subject == null)
            {
                TempData["Error"] = "Subject not found.";
                return RedirectToAction("AddMark", new { id = markviewmodel.TaskId });
            }

            if (markviewmodel.TheoryMarks < 0 || markviewmodel.TheoryMarks > subject.MaxTheoryMarks ||
                markviewmodel.LabMarks < 0 || markviewmodel.LabMarks > subject.MaxLabMarks ||
                markviewmodel.InternalMarks < 0 || markviewmodel.InternalMarks > subject.MaxInternalMarks)
            {
                TempData["Error"] = $"Invalid Marks! Marks Cannot Above than Theory({subject.MaxTheoryMarks}), Lab({subject.MaxLabMarks}), Internal({subject.MaxInternalMarks})";

                return RedirectToAction("AddMark", new { id = markviewmodel.TaskId });
            }


            if (existingMark != null)
            {
                existingMark.TheoryMarks = markviewmodel.TheoryMarks;
                existingMark.LabMarks = markviewmodel.LabMarks;
                existingMark.InternalMarks = markviewmodel.InternalMarks;
            }
            else
            {
                var newMark = new Marks
                {
                    TasksId = markviewmodel.TaskId,
                    StudentId = markviewmodel.StudentId,
                    SubjectId = markviewmodel.SubjectId,
                    TheoryMarks = markviewmodel.TheoryMarks,
                    LabMarks = markviewmodel.LabMarks,
                    InternalMarks = markviewmodel.InternalMarks
                };


                _context.Marks.Add(newMark);
            }


            _context.SaveChanges();
            TempData["Success"] = "Marks saved successfully!";
            return RedirectToAction("AddMark", new { id = markviewmodel.TaskId });
        }

        public IActionResult CompleteTask(int taskId)
        {
            var task = _context.Tasks.Find(taskId);
            if (task != null)
            {
                task.Status = Status.Completed;
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("AddMark", new { taskId });
        }
       
        [HttpGet]
        public IActionResult SendLateRequest(int id)
        {
            var task = _context.Tasks.Find(id);

            if (task.Status != Status.Overdue)
                return Unauthorized();

            return View(task);
        }

        [HttpPost]
        public IActionResult SendLateRequest(int TasksId, string Description)
        {
            var task = _context.Tasks.Find(TasksId);

            if (task.Status != Status.Overdue)
                return Unauthorized();

            task.Description = Description;
            task.Status = Status.LateRequested;

            _context.SaveChanges();

            return RedirectToAction("MyTasks");
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var staff = _context.Staffs
                .FirstOrDefault(s => s.AppUserId == userId);

            if (staff == null)
                return RedirectToAction("Login", "Account");

            var vm = new EditStaff
            {
                StaffId = staff.StaffId,
                StaffName = staff.Name,
                Email = staff.Email,
                ProfileImage = staff.ProfileImage,
                MobileNo = staff.MobileNo
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> EditProfile(EditStaffVM sm)
        {
            if (!ModelState.IsValid)
                return View(sm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var staff = _context.Staffs.Where(s => s.AppUserId == userId).FirstOrDefault();
            staff.Email = sm.Email;
            staff.MobileNo = sm.MobileNo;
            staff.Name = sm.StaffName;
            staff.ProfileImage = await SaveProfileImageAsync(sm.ProfileImage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }



        private async Task<string> SaveProfileImageAsync(IFormFile? profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
                return string.Empty;

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}_{profileImage.FileName}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            // Ensure uploads folder exists
            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath)!);

            // Save file
            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            // Return relative path to store in DB
            return $"/uploads/{fileName}";

        }
    }
}
