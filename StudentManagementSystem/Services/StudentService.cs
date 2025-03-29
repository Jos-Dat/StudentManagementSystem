using StudentManagementSystem.Models;
using StudentManagementSystem.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IAuthService _authService;

        public StudentService(
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IRepository<Enrollment> enrollmentRepository,
            IAuthService authService)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _authService = authService;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _studentRepository.GetStudentsWithUsersAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _studentRepository.GetStudentWithUserAsync(id);
        }

        public async Task<Student> GetStudentByUserIdAsync(int userId)
        {
            var students = await _studentRepository.FindAsync(s => s.UserId == userId);
            return students.FirstOrDefault();
        }

        public async Task<Student> GetStudentByStudentNumberAsync(string studentNumber)
        {
            return await _studentRepository.GetByStudentNumberAsync(studentNumber);
        }

        public async Task<bool> CreateStudentAsync(Student student, string password)
        {
            // Check if student number already exists
            if (await _studentRepository.ExistsAsync(s => s.StudentNumber == student.StudentNumber))
                return false;

            // Create user account for student
            var user = new User
            {
                Username = student.StudentNumber,
                Email = student.User.Email,
                FirstName = student.User.FirstName,
                LastName = student.User.LastName,
                Role = UserRole.Student,
                IsActive = true
            };

            // Register user account
            var success = await _authService.RegisterUserAsync(user, password);
            if (!success)
                return false;

            // Set the user ID in student entity
            student.UserId = user.Id;
            student.User = null; // Avoid duplicate entry

            // Add student record
            await _studentRepository.AddAsync(student);
            return true;
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            var existingStudent = await _studentRepository.GetStudentWithUserAsync(student.Id);
            if (existingStudent == null)
                return false;

            // Update user information
            existingStudent.User.FirstName = student.User.FirstName;
            existingStudent.User.LastName = student.User.LastName;
            existingStudent.User.Email = student.User.Email;

            // Update student information
            existingStudent.Address = student.Address;
            existingStudent.PhoneNumber = student.PhoneNumber;
            existingStudent.DateOfBirth = student.DateOfBirth;

            await _userRepository.UpdateAsync(existingStudent.User);
            await _studentRepository.UpdateAsync(existingStudent);
            return true;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _studentRepository.GetStudentWithUserAsync(id);
            if (student == null)
                return false;

            // Check if student has enrollments
            var enrollments = await _enrollmentRepository.FindAsync(e => e.StudentId == id);
            if (enrollments.Any())
            {
                // Deactivate user account instead of deletion
                student.User.IsActive = false;
                await _userRepository.UpdateAsync(student.User);
                return true;
            }

            // Delete student and user records
            await _studentRepository.DeleteAsync(student);
            await _userRepository.DeleteAsync(student.User);
            return true;
        }

        public async Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId)
        {
            return await _studentRepository.GetStudentsByCourseAsync(courseId);
        }

        public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
        {
            var enrollments = await _enrollmentRepository.FindAsync(e => e.StudentId == studentId);
            return enrollments.OrderByDescending(e => e.EnrollmentDate);
        }

        public async Task<IEnumerable<Course>> GetAvailableCoursesForStudentAsync(int studentId)
        {
            // Get all active courses
            var activeCourses = await _courseRepository.GetActiveCourses();

            // Get courses that student is already enrolled in
            var enrollments = await _enrollmentRepository.FindAsync(e => e.StudentId == studentId);
            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

            // Filter out courses that student is already enrolled in
            return activeCourses.Where(c => !enrolledCourseIds.Contains(c.Id));
        }
    }
}