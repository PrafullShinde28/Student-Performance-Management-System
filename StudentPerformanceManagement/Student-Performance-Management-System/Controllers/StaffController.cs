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

        public StaffController(UserManager<AppUser> userManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var myTasks = await _context.Tasks
                .Include(t => t.Course)
                .Include(t => t.Subject)
                .Include(t => t.CourseGroup)
                .Where(t => t.Staff.AppUserId == userId)
                .ToListAsync();

            var vm = new StaffDashViewModel
            {

                // StaffDashViewModel properties
                StaffId = userId,
                StaffName = user?.UserName,
                TaskCount = myTasks.Count,
                Tasks = myTasks
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


    }
}
