using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        #region Student

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
        public async Task<IActionResult> AddStaff(string name, string email, string mobileNo)
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

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View();
            }

            await _userManager.AddToRoleAsync(user, "Staff");

            
            var staff = new Staff
            {
                Name = name,
                Email = email,
                MobileNo = mobileNo,
                AppUserId = user.Id   
            };

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();   

            
            string finalPassword = $"{staff.StaffId}@Sunbeam";

            
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, finalPassword);

            TempData["Success"] = $"Staff added. Default Password: {finalPassword}";
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
                TempData["Error"] = "Cannot delete this staff member because tasks are assigned to them.";
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
        public IActionResult AddCourseGroup(AddCourseGroup model)
        {
            /*if (!ModelState.IsValid)
            {
                
                model.Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.CourseName
                    }).ToList();

                return View(model);
            }*/

            var group = new CourseGroup
            {
                GroupName = model.CourseGroupName,
                CourseId = model.CourseId
            };

            _context.CourseGroups.Add(group);
            _context.SaveChanges();

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
            //if (!ModelState.IsValid)
            //{
            //    ViewBag.Courses = _context.Courses
            //        .Select(c => new SelectListItem
            //        {
            //            Value = c.CourseId.ToString(),
            //            Text = c.CourseName
            //        }).ToList();

            //    return View(model);
            //}

            var group = _context.CourseGroups.Find(model.CourseGroupId);
            if (group == null)
                return NotFound();

            group.GroupName = model.GroupName;
            group.CourseId = model.CourseId;

            _context.SaveChanges();

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
                TempData["Error"] = "Cannot delete this course group because students are assigned to it.";
                return RedirectToAction("CourseGroups");
            }

            if (hasTasks)
            {
                TempData["Error"] = "Cannot delete this course group because tasks are assigned to it.";
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
            return RedirectToAction("Subjects");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubjects(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.SubjectId == id);

            if (subject == null)
                return NotFound();

            _context.Subjects.Remove(subject);
            _context.SaveChanges();

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

            return RedirectToAction("Subjects");
        }
        #endregion

        #region tasks
        public IActionResult Tasks()
        {
            var tlist = _db.Tasks.Include(t => t.Course).Include(t => t.Subject).Include(t => t.CourseGroup).Include(t => t.Staff).ToList();
            return View(tlist);
        }

        [HttpGet]
        public IActionResult AddTask()
        {

            var data = new AddTasksViewModel
            {
                Courses = _db.Courses.Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.CourseName }).ToList(),
                CourseGroups = _db.CourseGroups.Select(c => new SelectListItem { Value = c.CourseGroupId.ToString(), Text = c.GroupName }).ToList(),
                Subjects = _db.Subjects.Select(s => new SelectListItem { Value = s.SubjectId.ToString(), Text = s.SubjectName }).ToList(),
                Staffs = _db.Staffs.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name }).ToList()
            };
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(AddTask t)
        {
            var task = new Tasks()
            {
                Title = t.Title,
                Description = t.Description,
                StaffId = t.StaffId,
                CourseId = t.CourseId,
                SubjectId = t.SubjectId,
                CourseGroupId = t.CourseGroupId,
                ValidFrom = t.ValidFrom,
                ValidTo = t.ValidTo,
                Status = Status.Pending
            };
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return RedirectToAction("Tasks", "Admin");
        }
        public IActionResult GetSubjectsByCourse(int courseId)
        {
            var subjects = _context.Subjects
        .Where(s => s.CourseId == courseId)
        .Select(s => new
        {
            subjectId = s.SubjectId,
            subjectName = s.SubjectName
        })
        .ToList();

            return Json(subjects);
        }

        public IActionResult GetGroupsByCourse(int courseId)
        {
            var subjects = _context.CourseGroups
        .Where(s => s.CourseId == courseId)
        .Select(s => new
        {
            courseGroupId = s.CourseGroupId,
            groupName = s.GroupName
        })
        .ToList();

            return Json(subjects);
        }

        [HttpGet]
        public JsonResult GetSubjectsByCourses(int courseId)
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

        public IActionResult EditTask(int id)
        {
            var t = _db.Tasks.Find(id);
            ViewBag.TaskTitle = t.Title;
            ViewBag.Description = t.Description;
            ViewBag.Id = t.TasksId;
            var data = new AddTasksViewModel
            {
                Courses = _db.Courses.Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.CourseName }).ToList(),
                CourseGroups = _db.CourseGroups.Select(c => new SelectListItem { Value = c.CourseGroupId.ToString(), Text = c.GroupName }).ToList(),
                Subjects = _db.Subjects.Select(s => new SelectListItem { Value = s.SubjectId.ToString(), Text = s.SubjectName }).ToList(),
                Staffs = _db.Staffs.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name }).ToList()
            };
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(EditTaskVM t)
        {
            var tsk = _db.Tasks.Find(t.Id);
            tsk.Title = t.Title;
            tsk.Description = t.Description;
            tsk.CourseGroupId = t.CourseGroupId;
            tsk.SubjectId = t.SubjectId;
            tsk.StaffId = t.StaffId;
            tsk.CourseId = t.CourseId;
            tsk.ValidFrom = t.ValidFrom;
            tsk.ValidTo = t.ValidTo;
            await _db.SaveChangesAsync();
            return RedirectToAction("Tasks", "Admin");
        }


        public  IActionResult DeleteTask(int id)
        {
            var t = _db.Tasks.Find(id);
            _db.Tasks.Remove(t);
            _db.SaveChanges();
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

                    if (m.TheoryMarks < 40) failed += "T";
                    if (m.LabMarks < 20) failed += "L";
                    if (m.InternalMarks < 20) failed += "I";

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
                    if (m.TheoryMarks < 16) failed += "T";
                    if (m.LabMarks < 16) failed += "L";
                    if (m.InternalMarks < 8) failed += "I";

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

    }
}
