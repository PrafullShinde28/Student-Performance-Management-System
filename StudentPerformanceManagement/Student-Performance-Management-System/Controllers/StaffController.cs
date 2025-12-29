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
