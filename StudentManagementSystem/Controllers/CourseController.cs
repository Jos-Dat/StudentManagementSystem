using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;

        public CourseController(ICourseService courseService, IAuthService authService)
        {
            _courseService = courseService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses.Where(c => c.IsActive).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Check if user is admin or faculty
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userRole = _authService.GetUserRoleFromToken(token);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                IsAdminOrFaculty = userRole == UserRole.Admin || userRole == UserRole.Faculty
            };

            if (viewModel.IsAdminOrFaculty)
            {
                viewModel.EnrolledStudents = (await _courseService.GetEnrolledStudentsAsync(id)).ToList();
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet]
        public async Task<IActionResult> ManageStudents(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var students = await _courseService.GetEnrolledStudentsAsync(id);
            var viewModel = new ManageCourseStudentsViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseCode = course.CourseCode,
                EnrolledStudents = students.ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Faculty")]
        [HttpPost]
        public async Task<IActionResult> UpdateGrade(UpdateGradeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ManageStudents", new { id = model.CourseId });
            }

            var result = await _courseService.UpdateStudentGradeAsync(model.EnrollmentId, model.Grade);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to update grade.";
                return RedirectToAction("ManageStudents", new { id = model.CourseId });
            }

            TempData["SuccessMessage"] = "Grade updated successfully.";
            return RedirectToAction("ManageStudents", new { id = model.CourseId });
        }

        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet]
        public async Task<IActionResult> FacultyCourses()
        {
            // Get current faculty's user ID
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _courseService.GetCoursesByFacultyAsync(userId.Value);
            return View(courses.ToList());
        }
    }
}