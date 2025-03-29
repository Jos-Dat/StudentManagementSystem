using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;

        public StudentController(
            IStudentService studentService,
            ICourseService courseService,
            IAuthService authService)
        {
            _studentService = studentService;
            _courseService = courseService;
            _authService = authService;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            // Get student enrollments
            var enrollments = await _studentService.GetStudentEnrollmentsAsync(student.Id);

            var dashboardViewModel = new StudentDashboardViewModel
            {
                Student = student,
                CurrentEnrollments = enrollments.Where(e => e.Status == "Enrolled").ToList(),
                CompletedCourses = enrollments.Where(e => e.Status == "Completed").ToList()
            };

            return View(dashboardViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CourseRegistration()
        {
            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            // Get available courses
            var availableCourses = await _studentService.GetAvailableCoursesForStudentAsync(student.Id);

            var viewModel = new CourseRegistrationViewModel
            {
                Student = student,
                AvailableCourses = availableCourses.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EnrollCourse(EnrollCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("CourseRegistration");
            }

            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            // Enroll in course
            var result = await _courseService.EnrollStudentAsync(model.CourseId, student.Id, model.Semester, model.AcademicYear);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to enroll in course. The course may be full or you may already be enrolled.";
                return RedirectToAction("CourseRegistration");
            }

            TempData["SuccessMessage"] = "Enrolled in course successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Transcript()
        {
            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            // Get all enrollments
            var enrollments = await _studentService.GetStudentEnrollmentsAsync(student.Id);

            var viewModel = new TranscriptViewModel
            {
                Student = student,
                Enrollments = enrollments.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UnenrollCourse(int enrollmentId)
        {
            var result = await _courseService.UnenrollStudentAsync(enrollmentId);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to drop course.";
                return RedirectToAction("Dashboard");
            }

            TempData["SuccessMessage"] = "Course dropped successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            var viewModel = new StudentProfileViewModel
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                FirstName = student.User.FirstName,
                LastName = student.User.LastName,
                Email = student.User.Email,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                EnrollmentDate = student.EnrollmentDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            // Get current student
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _studentService.GetStudentByUserIdAsync(userId.Value);
            if (student == null)
            {
                return NotFound();
            }

            // Update student information
            student.Address = model.Address;
            student.PhoneNumber = model.PhoneNumber;
            student.DateOfBirth = model.DateOfBirth;
            student.User.FirstName = model.FirstName;
            student.User.LastName = model.LastName;
            student.User.Email = model.Email;

            var result = await _studentService.UpdateStudentAsync(student);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
                return View("Profile", model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }
    }
}