using StudentManagementSystem.Models;
using StudentManagementSystem.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;

        public CourseService(
            ICourseRepository courseRepository,
            IStudentRepository studentRepository,
            IRepository<Enrollment> enrollmentRepository)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task<Course> GetCourseByCourseCodeAsync(string courseCode)
        {
            return await _courseRepository.GetByCourseCodeAsync(courseCode);
        }

        public async Task<bool> CreateCourseAsync(Course course)
        {
            // Check if course code already exists
            if (await _courseRepository.ExistsAsync(c => c.CourseCode == course.CourseCode))
                return false;

            await _courseRepository.AddAsync(course);
            return true;
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            var existingCourse = await _courseRepository.GetByIdAsync(course.Id);
            if (existingCourse == null)
                return false;

            // Check if course code is changed and already exists
            if (course.CourseCode != existingCourse.CourseCode &&
                await _courseRepository.ExistsAsync(c => c.CourseCode == course.CourseCode))
                return false;

            existingCourse.CourseCode = course.CourseCode;
            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            existingCourse.Credits = course.Credits;
            existingCourse.MaxStudents = course.MaxStudents;
            existingCourse.FacultyId = course.FacultyId;
            existingCourse.Semester = course.Semester;
            existingCourse.IsActive = course.IsActive;

            await _courseRepository.UpdateAsync(existingCourse);
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return false;

            // Check if course has enrollments
            var enrollments = await _enrollmentRepository.FindAsync(e => e.CourseId == id);
            if (enrollments.Any())
            {
                // Deactivate course instead of deletion
                course.IsActive = false;
                await _courseRepository.UpdateAsync(course);
                return true;
            }

            await _courseRepository.DeleteAsync(course);
            return true;
        }

        public async Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId)
        {
            return await _courseRepository.GetCoursesByFacultyAsync(facultyId);
        }

        public async Task<IEnumerable<Course>> GetCoursesBySemesterAsync(string semester)
        {
            return await _courseRepository.GetCoursesBySemesterAsync(semester);
        }

        public async Task<IEnumerable<Student>> GetEnrolledStudentsAsync(int courseId)
        {
            return await _studentRepository.GetStudentsByCourseAsync(courseId);
        }

        public async Task<bool> EnrollStudentAsync(int courseId, int studentId, string semester, string academicYear)
        {
            // Check if course exists and is active
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null || !course.IsActive)
                return false;
            // Check if student exists
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return false;
            // Check if student is already enrolled in this course
            if (await _enrollmentRepository.ExistsAsync(e => e.CourseId == courseId && e.StudentId == studentId))
                return false;
            // Check if course has reached maximum enrollment
            var currentEnrollments = await _enrollmentRepository.CountAsync(e => e.CourseId == courseId);
            if (currentEnrollments >= course.MaxStudents)
                return false;
            // Create enrollment
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrollmentDate = DateTime.Now,
                Status = "Enrolled",
                Semester = semester,
                AcademicYear = academicYear
            };
            await _enrollmentRepository.AddAsync(enrollment);
            return true;
        }

        public async Task<bool> UpdateStudentGradeAsync(int enrollmentId, decimal grade)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.Grade = grade;
            enrollment.Status = grade >= 50 ? "Completed" : "Failed";

            await _enrollmentRepository.UpdateAsync(enrollment);
            return true;
        }

        public async Task<bool> UnenrollStudentAsync(int enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.Status = "Dropped";
            await _enrollmentRepository.UpdateAsync(enrollment);
            return true;
        }

        public async Task<IEnumerable<Course>> GetCoursesWithEnrollmentCountAsync()
        {
            return await _courseRepository.GetCoursesWithEnrollmentCountAsync();
        }
    }
}