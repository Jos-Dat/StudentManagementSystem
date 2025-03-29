using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.ViewModels;
using System.Threading.Tasks;

namespace StudentManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IStudentService _studentService;

        public AccountController(IAuthService authService, IStudentService studentService)
        {
            _authService = authService;
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Create authentication cookie
            var token = _authService.GenerateJwtToken(user);
            HttpContext.Response.Cookies.Append("AuthToken", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
            });

            // Redirect based on user role
            switch (user.Role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Dashboard", "Admin");
                case UserRole.Faculty:
                    return RedirectToAction("Dashboard", "Home");
                case UserRole.Student:
                    return RedirectToAction("Dashboard", "Student");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create user entity
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = UserRole.Student, // Default role for registration is Student
                IsActive = true
            };

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View(model);
            }

            // Register student
            var student = new Student
            {
                StudentNumber = model.Username, // Using username as student number for simplicity
                User = user,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                EnrollmentDate = System.DateTime.Now
            };

            var result = await _studentService.CreateStudentAsync(student, model.Password);
            if (!result)
            {
                ModelState.AddModelError("", "Registration failed. Username or email may already be in use.");
                return View(model);
            }

            // Automatically log in the user after registration
            var authenticatedUser = await _authService.AuthenticateAsync(model.Username, model.Password);
            if (authenticatedUser != null)
            {
                var token = _authService.GenerateJwtToken(authenticatedUser);
                HttpContext.Response.Cookies.Append("AuthToken", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
            }

            return RedirectToAction("Dashboard", "Student");
        }

        [Authorize]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get current user ID from token
            var token = HttpContext.Request.Cookies["AuthToken"];
            var userId = _authService.GetUserIdFromToken(token);

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var result = await _authService.ChangePasswordAsync(userId.Value, model.CurrentPassword, model.NewPassword);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to change password. Current password may be incorrect.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}