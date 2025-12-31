using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Student_Performance_Management_System.Models;
using Student_Performance_Management_System.ViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Student_Performance_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AdminController(UserManager<AppUser> userManager,
                               ApplicationDbContext context, ApplicationDbContext db, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _db = db;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Student

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

        public IActionResult Students()
        {
            var students = _context.Students
                .Include(s => s.Course)
                .Include(s => s.CourseGroup)
                .ToList();

            return View(students);
        }



        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentId == id);
            if (student == null)
                return NotFound();

            var model = new EditStudentViewModel
            {
                StudentId = student.StudentId,
                AppUserId = student.AppUserId,
                PRN = student.PRN,
                Name = student.Name,
                Email = student.Email,
                MobileNo = student.MobileNo,
                CourseId = student.CourseId,
                CourseGroupId = student.CourseGroupId,

                Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Text = c.CourseName,
                    Value = c.CourseId.ToString()
                }).ToList(),

                CourseGroups = _context.CourseGroups
                    .Where(g => g.CourseId == student.CourseId)
                    .Select(g => new SelectListItem
                    {
                        Text = g.GroupName,
                        Value = g.CourseGroupId.ToString()
                    }).ToList()
            };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> EditStudent(EditStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Text = c.CourseName,
                    Value = c.CourseId.ToString()
                }).ToList();

                model.CourseGroups = _context.CourseGroups
                    .Where(g => g.CourseId == model.CourseId)
                    .Select(g => new SelectListItem
                    {
                        Text = g.GroupName,
                        Value = g.CourseGroupId.ToString()
                    }).ToList();

                return View(model);
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == model.StudentId);
            if (student == null)
                return NotFound();

            student.Name = model.Name;
            student.MobileNo = model.MobileNo;
            student.CourseId = model.CourseId;
            student.CourseGroupId = model.CourseGroupId;

            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Student updated successfully.";
            return RedirectToAction("Students");
        }



        // DELETE STUDENT (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentId == id);
            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Students");
            }

            var user = await _userManager.FindByIdAsync(student.AppUserId);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            if (user != null)
                await _userManager.DeleteAsync(user);

            TempData["Success"] = "Student deleted successfully.";
            return RedirectToAction("Students");
        }


        public string GeneratePRN()
        {
            int year = DateTime.Now.Year;
            //int year = 2026;
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
        public async Task<IActionResult> EnrollStudent(
                                                 StudentEnrollmentViewModel model)
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
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FullName = model.Name,
            };

            var result = await _userManager.CreateAsync(user, defaultPassword);

            string profileImagePath = await SaveProfileImageAsync(model.ProfileImage);


            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");

                var student = new Student
                {
                    PRN = GeneratePRN(),
                    Name = model.Name,
                    Email = model.Email,
                    AppUserId = user.Id,
                    MobileNo = model.MobileNo,
                    CourseId = model.CourseId,
                    CourseGroupId = model.CourseGroupId,
                    ProfileImagePath = profileImagePath
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Student enrolled successfully.";

                var subject = "Student enrolled sucessfully";
                var body = $@"
                          Dear, {model.Name} <br/>
                          You have been Enrolled successfully. <br/>
                          UserName: <b> {student.PRN} </b><br/>
                          Password: <b>{defaultPassword}  </b><br/><br/>
                          Regards, <br/>
                          Admin Team  
                        ";
                await _emailSender.SendEmailAsync(model.Email, subject, body);
                return RedirectToAction("Students");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult GetGroupsByCourseId(int courseId)
        {
            Console.WriteLine("CourseId = " + courseId);

            var groups = _context.CourseGroups
                .Where(g => g.CourseId == courseId)
                .Select(g => new
                {
                    id = g.CourseGroupId,
                    name = g.GroupName
                })
                .ToList();

            return Json(groups);
        }





        #endregion

        #region  Staff
        // Staff
        public IActionResult Staff()
        {
            var staffs = _context.Staffs.ToList();
            return View(staffs);
        }



        // ADD STAFF (GET)
        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }

        // ADD STAFF (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(string name, string email, string mobileNo, IFormFile ProfileImage)
        {
            var tempPassword = "Temp@123";

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, tempPassword);
            string profileImagePath = await SaveProfileImageAsync(ProfileImage);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors.First().Description;
                return RedirectToAction("AddStaff");
            }

            await _userManager.AddToRoleAsync(user, "Staff");

            var staff = new Staff
            {
                Name = name,
                Email = email,
                MobileNo = mobileNo,
                AppUserId = user.Id,
                ProfileImage = profileImagePath

            };

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            string finalPassword = $"{staff.StaffId}@Sunbeam";

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, finalPassword);

            TempData["Success"] = $"Staff added successfully. Password: {finalPassword}";
            return RedirectToAction("Staff");
        }


        [HttpGet]
        public IActionResult EditStaff(int id)
        {
            var staff = _context.Staffs
                .Include(s => s.Tasks)
                .FirstOrDefault(s => s.StaffId == id);

            if (staff == null)
                return NotFound();

            return View(staff);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStaff(Staff model)
        {
            var staff = _context.Staffs
                .Include(s => s.Tasks)
                .FirstOrDefault(s => s.StaffId == model.StaffId);

            if (staff == null)
                return NotFound();

            staff.Name = model.Name;
            staff.MobileNo = model.MobileNo;

            _context.SaveChanges();
            TempData["Success"] = "Staff updated successfully.";
            return RedirectToAction("Staff");
        }



        //Delete staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffId == id);
            if (staff == null)
                return NotFound();

            bool hasTasks = _context.Tasks.Any(t => t.StaffId == id);

            if (hasTasks)
            {
                TempData["Error"] = "Cannot delete staff. Tasks are assigned.";
                return RedirectToAction("Staff");
            }

            var user = await _userManager.FindByIdAsync(staff.AppUserId);

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            if (user != null)
                await _userManager.DeleteAsync(user);

            TempData["Success"] = "Staff deleted successfully.";
            return RedirectToAction("Staff");
        }



        public IActionResult ViewStaffTasks(int id)
        {
            var staff = _context.Staffs
                .Include(s => s.Tasks)
                    .ThenInclude(t => t.Course)
                .Include(s => s.Tasks)
                    .ThenInclude(t => t.CourseGroup)
                .Include(s => s.Tasks)
                    .ThenInclude(t => t.Subject)
                .FirstOrDefault(s => s.StaffId == id);

            if (staff == null)
                return NotFound();

            var vm = new StaffTaskAssignedVM
            {
                StaffId = staff.StaffId,
                StaffName = staff.Name
            };

            foreach (var task in staff.Tasks)
            {
                vm.Tasks.Add(new TaskDetailsVM
                {
                    Title = task.Title,
                    Description = task.Description,
                    CourseName = task.Course.CourseName,
                    CourseGroupName = task.CourseGroup.GroupName,
                    SubjectName = task.Subject.SubjectName,
                    ValidFrom = task.ValidFrom,
                    ValidTo = task.ValidTo,
                    Status = task.Status.ToString()
                });
            }

            return View(vm);
        }


        #endregion

        #region Course 

        public IActionResult Courses()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }
        //Edit course
        [HttpGet]
        public IActionResult EditCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return View(course);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = _context.Courses.FirstOrDefault(c => c.CourseId == model.CourseId);
            if (course == null)
                return NotFound();

            course.CourseName = model.CourseName;
            course.Description = model.Description;
            course.Duration = model.Duration;
            course.Fees = model.Fees;

            _context.SaveChanges();

            TempData["Success"] = "Course updated successfully.";
            return RedirectToAction("Courses");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null)
                return NotFound();

            bool hasStudents = _context.Students.Any(s => s.CourseId == id);
            if (hasStudents)
            {
                TempData["Error"] = "Cannot delete course. Students are already enrolled.";
                return RedirectToAction("Courses");
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();

            TempData["Success"] = "Course deleted successfully.";
            return RedirectToAction("Courses");
        }


        // ADD COURSE (GET)
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        // ADD COURSE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCourse(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            _context.Courses.Add(course);
            _context.SaveChanges();

            TempData["Success"] = "Course added successfully.";
            return RedirectToAction("Courses");
        }


        // student list
        public IActionResult CourseStudents(int id)
        {
            var course = _context.Courses
                .FirstOrDefault(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            var vm = new CourseStudentsVM
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Students = _context.Students
                    .Where(s => s.CourseId == id)
                    .Select(s => new CourseStudentItemVM
                    {
                        PRN = s.PRN,
                        Name = s.Name,
                        Email = s.Email,
                        MobileNo = s.MobileNo,
                        CourseGroupName = s.CourseGroup.GroupName
                    })
                    .ToList()
            };

            return View(vm);
        }


        #endregion

        #region CourseGroup

        [HttpGet]
        public IActionResult CourseGroups()
        {
            var groups = _context.CourseGroups
                .Include(g => g.Course)
                .ToList();

            return View(groups);
        }

        // ADD (GET)
        [HttpGet]
        public IActionResult AddCourseGroup()
        {
            var vm = new AddCourseGroupVM
            {
                Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList()
            };

            return View(vm);
        }

        // ADD (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCourseGroup(AddCourseGroupVM model)
        {
           

            bool exists = _context.CourseGroups.Any(g =>
                g.GroupName == model.CourseGroupName &&
                g.CourseId == model.CourseId);

            if (exists)
            {
                TempData["Error"] = "This course group already exists.";
                return RedirectToAction("CourseGroups");
            }

            var group = new CourseGroup
            {
                GroupName = model.CourseGroupName,
                CourseId = model.CourseId.Value
            };

            _context.CourseGroups.Add(group);
            _context.SaveChanges();

            TempData["Success"] = "Course group added successfully.";
            return RedirectToAction("CourseGroups");
        }



        // EDIT (GET)
        [HttpGet]
        public IActionResult EditCourseGroup(int id)
        {
            var group = _context.CourseGroups.Find(id);
            if (group == null)
                return NotFound();

            ViewBag.Courses = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList();

            return View(group);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourseGroup(CourseGroup model)
        {
            

            var group = _context.CourseGroups.Find(model.CourseGroupId);
            if (group == null)
                return NotFound();

            bool duplicate = _context.CourseGroups.Any(g =>
                g.CourseGroupId != model.CourseGroupId &&
                g.GroupName == model.GroupName &&
                g.CourseId == model.CourseId);

            if (duplicate)
            {
                TempData["Error"] = "Another group with same name already exists.";
                return RedirectToAction("CourseGroups");
            }

            group.GroupName = model.GroupName;
            group.CourseId = model.CourseId;

            _context.SaveChanges();

            TempData["Success"] = "Course group updated successfully.";
            return RedirectToAction("CourseGroups");
        }


        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourseGroup(int id)
        {
            var group = _context.CourseGroups.Find(id);
            if (group == null)
                return NotFound();

            bool hasStudents = _context.Students.Any(s => s.CourseGroupId == id);
            bool hasTasks = _context.Tasks.Any(t => t.CourseGroupId == id);

            if (hasStudents)
            {
                TempData["Error"] = "Cannot delete course group. Students are assigned.";
                return RedirectToAction("CourseGroups");
            }

            if (hasTasks)
            {
                TempData["Error"] = "Cannot delete course group. Tasks are assigned.";
                return RedirectToAction("CourseGroups");
            }

            _context.CourseGroups.Remove(group);
            _context.SaveChanges();

            TempData["Success"] = "Course group deleted successfully.";
            return RedirectToAction("CourseGroups");
        }

        #endregion

        #region Subject
        public IActionResult Subjects()
        {
            var subjects = _context.Subjects
                            .Include(s => s.Course)
                            .ToList();

            return View(subjects);
        }

        [HttpGet]
        public IActionResult EditSubjects(int id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject == null) return NotFound();

            var vm = new SubjectCreateVM
            {
                SubjectName = subject.SubjectName,
                CourseId = subject.CourseId,
                CourseList = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList()
            };

            ViewBag.SubjectId = subject.SubjectId;
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubjects(int id, SubjectCreateVM vm)
        {
            ModelState.Remove(nameof(vm.CourseList));

            if (!ModelState.IsValid)
            {
                vm.CourseList = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList();

                return View(vm);
            }

            var subject = _context.Subjects.Find(id);
            if (subject == null) return NotFound();

            subject.SubjectName = vm.SubjectName;
            subject.CourseId = vm.CourseId;

            _context.SaveChanges();
            TempData["Success"] = "Subject updated successfully.";
            return RedirectToAction("Subjects");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubjects(int id)
        {
            try
            {
                var subject = _context.Subjects.FirstOrDefault(s => s.SubjectId == id);
                if (subject == null) return NotFound();

                _context.Subjects.Remove(subject);
                _context.SaveChanges();

                TempData["Success"] = "Subject deleted successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "This subject is already used in student marks or tasks and cannot be deleted.";
            }

            return RedirectToAction("Subjects");
        }




        [HttpGet]
        public IActionResult AddSubjects()
        {
            var vm = new SubjectCreateVM
            {
                CourseList = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSubjects(SubjectCreateVM vm)
        {
            ModelState.Remove(nameof(vm.CourseList));

            if (!ModelState.IsValid)
            {
                vm.CourseList = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList();

                return View(vm);
            }

            var subject = new Subject
            {
                SubjectName = vm.SubjectName,
                CourseId = vm.CourseId
            };

            _context.Subjects.Add(subject);
            _context.SaveChanges();
            TempData["Success"] = "Subject added successfully.";
            return RedirectToAction("Subjects");
        }
        #endregion

        #region tasks
        public IActionResult Tasks()
        {
            var tasks = _db.Tasks
                .Include(t => t.Course)
                .Include(t => t.Subject)
                .Include(t => t.CourseGroup)
                .Include(t => t.Staff)
                .ToList();

            return View(tasks);
        }

        private void UpdateOverdueTasks()
        {
            var now = DateTime.Now;

            var tasks = _context.Tasks.ToList();

            foreach (var t in tasks)
            {
                // Only normal pending tasks become overdue
                if (t.Status == Status.Pending && now > t.ValidTo)
                {
                    t.Status = Status.Overdue;
                }
            }

            _context.SaveChanges();
        }


        public IActionResult LateRequests()
        {
            UpdateOverdueTasks();

            var list = _db.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Subject)
                .Include(t => t.Course)
                .Where(t => t.Status == Status.LateRequested)
                .ToList();

            return View(list);
        }

        public IActionResult ApproveLate(int id)
        {
            var task = _db.Tasks.Find(id);

            task.ValidTo = task.ValidTo.AddDays(3);
            task.Status = Status.Pending;


            _db.SaveChanges();
            return RedirectToAction("LateRequests");
        }


        public IActionResult RejectLate(int id)
        {
            var task = _db.Tasks.Find(id);
            task.Status = Status.LateRejected;
            _db.SaveChanges();
            return RedirectToAction("LateRequests");
        }

        [HttpGet]
        public IActionResult AddTask()
        {
            var vm = new AddTasksViewModel
            {
                Courses = _db.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList(),

                Staffs = _db.Staffs
                    .Select(s => new SelectListItem
                    {
                        Value = s.StaffId.ToString(),
                        Text = s.Name
                    }).ToList(),

                CourseGroups = new List<SelectListItem>(),
                Subjects = new List<SelectListItem>()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(AddTask model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all required fields.";
                return RedirectToAction("AddTask");
            }

           
            bool exists = _db.Tasks.Any(t =>
                t.SubjectId == model.SubjectId &&
                t.CourseGroupId == model.CourseGroupId
            );

            if (exists)
            {
                TempData["Error"] = "Task already exists for this Subject and Course Group.";
                return RedirectToAction("Tasks");
            }

            var task = new Tasks
            {
                Title = model.Title,
                Description = model.Description,
                StaffId = model.StaffId,
                CourseId = model.CourseId,
                CourseGroupId = model.CourseGroupId,
                SubjectId = model.SubjectId,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                Status = Status.Pending
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Task added successfully.";
            return RedirectToAction("Tasks");
        }


        [HttpGet]
        public IActionResult GetSubjectsByCourse(int courseId)
        {
            var subjects = _db.Subjects
                .Where(s => s.CourseId == courseId)
                .Select(s => new
                {
                    subjectId = s.SubjectId,
                    subjectName = s.SubjectName
                }).ToList();

            return Json(subjects);
        }


        [HttpGet]
        public IActionResult GetGroupsByCourse(int courseId)
        {
            var groups = _db.CourseGroups
                .Where(g => g.CourseId == courseId)
                .Select(g => new
                {
                    courseGroupId = g.CourseGroupId,
                    groupName = g.GroupName
                }).ToList();

            return Json(groups);
        }

        [HttpGet]
        public IActionResult EditTask(int id)
        {
            var task = _db.Tasks.Find(id);
            if (task == null)
            {
                TempData["Error"] = "Task not found.";
                return RedirectToAction("Tasks");
            }

            ViewBag.TaskTitle = task.Title;
            ViewBag.Description = task.Description;
            ViewBag.Id = task.TasksId;

            var vm = new AddTasksViewModel
            {
                Courses = _db.Courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList(),

                Staffs = _db.Staffs.Select(s => new SelectListItem
                {
                    Value = s.StaffId.ToString(),
                    Text = s.Name
                }).ToList(),

                CourseGroups = _db.CourseGroups.Select(g => new SelectListItem
                {
                    Value = g.CourseGroupId.ToString(),
                    Text = g.GroupName
                }).ToList(),

                Subjects = _db.Subjects.Select(s => new SelectListItem
                {
                    Value = s.SubjectId.ToString(),
                    Text = s.SubjectName
                }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(EditTaskVM model)
        {
            var task = _db.Tasks.Find(model.Id);
            if (task == null)
            {
                TempData["Error"] = "Task not found.";
                return RedirectToAction("Tasks");
            }

            
            bool exists = _db.Tasks.Any(t =>
                t.SubjectId == model.SubjectId &&
                t.CourseGroupId == model.CourseGroupId &&
                t.TasksId != model.Id
            );

            if (exists)
            {
                TempData["Error"] = "Another task already exists for this Subject and Course Group.";
                return RedirectToAction("Tasks", new { id = model.Id });
            }

            task.Title = model.Title;
            task.Description = model.Description;
            task.CourseId = model.CourseId;
            task.CourseGroupId = model.CourseGroupId;
            task.SubjectId = model.SubjectId;
            task.StaffId = model.StaffId;
            task.ValidFrom = model.ValidFrom;
            task.ValidTo = model.ValidTo;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction("Tasks");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTask(int id)
        {
            var task = _db.Tasks.Find(id);
            if (task == null)
            {
                TempData["Error"] = "Task not found.";
                return RedirectToAction("Tasks");
            }

            _db.Tasks.Remove(task);
            _db.SaveChanges();

            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction("Tasks");
        }


        #endregion

        #region report
        [HttpGet]
        public JsonResult GetSubjectByCourse(int courseId)
        {
            var subjects = _context.Subjects
                .Where(s => s.CourseId == courseId)
                .Select(s => new
                {
                    s.SubjectId,
                    s.SubjectName
                }).ToList();

            return Json(subjects);
        }

        [HttpGet]
        public IActionResult SubjectWiseReport()
        {
            var model = new SubjectWiseReportVM
            {
                Courses = _context.Courses.Include(c => c.Subjects).Select(c => new SelectListItem
                {
                    Text = c.CourseName,
                    Value = c.CourseId.ToString()
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SubjectWiseReport(SubjectWiseReportVM model)
        {
            model.Courses = _context.Courses.Select(c => new SelectListItem
            {
                Text = c.CourseName,
                Value = c.CourseId.ToString()
            }).ToList();

            var list = _context.Marks
                .Include(m => m.Student)
                .Where(m => m.SubjectId == model.SubjectId)
                .ToList()
                .Select(m =>
                {
                    string failed = "";

                    if (m.TheoryMarks < 15) failed += "T";
                    if (m.LabMarks < 15) failed += "L";
                    if (m.InternalMarks < 7) failed += "I";

                    bool isPass = failed == "";

                    return new StudentMarksRowVM
                    {
                        PRN = m.Student.PRN,
                        StudentName = m.Student.Name,
                        TheoryMarks = m.TheoryMarks,
                        LabMarks = m.LabMarks,
                        InternalMarks = m.InternalMarks,
                        TotalMarks = 100,
                        ObtainedMarks = m.TheoryMarks + m.LabMarks + m.InternalMarks,
                        FailedIn = failed,
                        ResultStatus = isPass ? "Pass" : "Fail"
                    };
                })
                .OrderByDescending(x => x.ObtainedMarks)
                .ToList();


            int rank = 1;
            int prevMarks = -1;
            int skip = 0;

            foreach (var item in list)
            {
                if (item.ObtainedMarks == prevMarks)
                {
                    item.Rank = rank;
                    skip++;
                }
                else
                {
                    rank += skip;
                    item.Rank = rank;
                    skip = 1;
                    prevMarks = item.ObtainedMarks;
                }
            }

            model.ReportRows = list;
            return View(model);
        }



        [HttpGet]
        public IActionResult CourseWiseReport()
        {
            var model = new CourseWiseReportVM
            {
                Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Text = c.CourseName,
                    Value = c.CourseId.ToString()
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult CourseWiseReport(CourseWiseReportVM model)
        {
            model.Courses = _context.Courses.Select(c => new SelectListItem
            {
                Text = c.CourseName,
                Value = c.CourseId.ToString()
            }).ToList();

            var subjectNames = _context.Subjects
                .Where(s => s.CourseId == model.CourseId)
                .Select(s => s.SubjectName)
                .ToList();

            var students = _context.Students
                .Include(s => s.Marks)
                .ThenInclude(m => m.Subject)
                .Where(s => s.CourseId == model.CourseId)
                .ToList();

            var list = students.Select(s =>
            {
                var marksList = new List<SubjectMarksVM>();
                int total = 0;
                bool courseFail = false;

                foreach (var m in s.Marks)
                {
                    string failed = "";
                    if (m.TheoryMarks < 15) failed += "T";
                    if (m.LabMarks < 15) failed += "L";
                    if (m.InternalMarks < 7) failed += "I";

                    if (failed != "") courseFail = true;

                    marksList.Add(new SubjectMarksVM
                    {
                        SubjectName = m.Subject.SubjectName,
                        Theory = m.TheoryMarks,
                        Lab = m.LabMarks,
                        Internal = m.InternalMarks,
                        FailedIn = failed
                    });

                    total += m.TheoryMarks + m.LabMarks + m.InternalMarks;
                }

                return new StudentRankingRowVM
                {
                    PRN = s.PRN,
                    StudentName = s.Name,
                    SubjectMarks = marksList,
                    TotalMarks = total,
                    Percentage = Math.Round((double)total / (subjectNames.Count * 100) * 100, 2),
                    ResultStatus = courseFail ? "FAIL" : "PASS"
                };
            })
            .OrderByDescending(x => x.TotalMarks)
            .ToList();

            // Rank with ties
            int rank = 1, prev = -1, skip = 0;
            foreach (var item in list)
            {
                if (item.TotalMarks == prev)
                {
                    item.Rank = rank;
                    skip++;
                }
                else
                {
                    rank += skip;
                    item.Rank = rank;
                    skip = 1;
                    prev = item.TotalMarks;
                }
            }

            model.SubjectNames = subjectNames;
            model.RankingRows = list;
            return View(model);
        }
        #endregion

        #region Performance card
        public int GetStudentRank(int studentId, int courseId)
        {
            var students = _db.Students.Where(s => s.CourseId == courseId).Include(s => s.Marks).ToList();
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
            var student = _db.Students
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
                    MaxMarks = m.Subject.MaxTheoryMarks + m.Subject.MaxLabMarks + m.Subject.MaxInternalMarks,
                    FailedIn = m.FailedIn(),
                }).ToList()
            };

            return View(vm);
        }

        #endregion

    }
}
