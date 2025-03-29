using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly IAuthService _authService;

        public HomeController(
            ILogger<HomeController> logger,
            ICourseService courseService,
            IAuthService authService)
        {
            _logger = logger;
            _courseService = courseService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            // Get current active courses for homepage
            var courses = await _courseService.GetAllCoursesAsync();
            var activeCourses = courses.Where(c => c.IsActive).Take(5).ToList();

            // Check if user is authenticated
            var token = HttpContext.Request.Cookies["AuthToken"];
            var isAuthenticated = !string.IsNullOrEmpty(token) && await _authService.ValidateTokenAsync(token);

            ViewBag.IsAuthenticated = isAuthenticated;

            if (isAuthenticated)
            {
                var userRole = _authService.GetUserRoleFromToken(token);
                ViewBag.UserRole = userRole;
            }

            return View(activeCourses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}