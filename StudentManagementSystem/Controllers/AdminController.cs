using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Repositories;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;

        public AdminController(
            IUserRepository userRepository,
            IStudentService studentService,
            ICourseService courseService,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _studentService = studentService;
            _courseService = courseService;
            _authService = authService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboardViewModel = new AdminDashboardViewModel
            {
                TotalStudents = await _userRepository.CountAsync(u => u.Role == UserRole.Student),
                TotalFaculty = await _userRepository.CountAsync(u => u.Role == UserRole.Faculty),
                TotalCourses = await _courseService.GetAllCoursesAsync().ContinueWith(t => t.Result.Count()),
                RecentStudents = (await _studentService.GetAllStudentsAsync()).Take(5).ToList()
            };

            return View(dashboardViewModel);
        }

        #region User Management

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role,
                IsActive = true
            };

            var result = await _authService.RegisterUserAsync(user, model.Password);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to create user. Username or email may already be in use.");
                return View(model);
            }

            // If user is a student, create student record
            if (model.Role == UserRole.Student)
            {
                var student = new Student
                {
                    StudentNumber = model.Username, // Using username as student number
                    UserId = user.Id,
                    EnrollmentDate = System.DateTime.Now
                };

                await _studentService.CreateStudentAsync(student, model.Password);
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            await _userRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // If user is a student, handle student record
            if (user.Role == UserRole.Student)
            {
                var student = await _studentService.GetStudentByUserIdAsync(id);
                if (student != null)
                {
                    await _studentService.DeleteStudentAsync(student.Id);
                    return RedirectToAction("ManageUsers");
                }
            }

            // For non-student users, just delete the user
            await _userRepository.DeleteAsync(user);
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction("ManageUsers");
        }

        #endregion

        #region Course Management

        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course)
        {

            Console.WriteLine("CreateCourse POST method reached");

            if (!ModelState.IsValid)
            {
                return View(course);
            }

            var result = await _courseService.CreateCourseAsync(course);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to create course. Course code may already be in use.");
                return View(course);
            }

            TempData["SuccessMessage"] = "Course created successfully.";
            return RedirectToAction("ManageCourses");
        }
        [HttpPost]
        public IActionResult TestCreate([FromBody] Course course)
        {
            return Json(new { success = true, message = "Test successful", data = course });
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            var result = await _courseService.UpdateCourseAsync(course);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to update course. Course code may already be in use.");
                return View(course);
            }

            TempData["SuccessMessage"] = "Course updated successfully.";
            return RedirectToAction("ManageCourses");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to delete course.";
                return RedirectToAction("ManageCourses");
            }

            TempData["SuccessMessage"] = "Course deleted successfully.";
            return RedirectToAction("ManageCourses");
        }

        [HttpGet]
        public async Task<IActionResult> ViewEnrollments(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var students = await _courseService.GetEnrolledStudentsAsync(id);
            var model = new CourseEnrollmentsViewModel
            {
                Course = course,
                EnrolledStudents = students.ToList()
            };

            return View(model);
        }

        #endregion
    }
}