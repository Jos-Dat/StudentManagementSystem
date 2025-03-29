using StudentManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task<Course> GetCourseByCourseCodeAsync(string courseCode);
        Task<bool> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId);
        Task<IEnumerable<Course>> GetCoursesBySemesterAsync(string semester);
        Task<IEnumerable<Student>> GetEnrolledStudentsAsync(int courseId);
        Task<bool> EnrollStudentAsync(int courseId, int studentId, string semester, string academicYear);
        Task<bool> UpdateStudentGradeAsync(int enrollmentId, decimal grade);
        Task<bool> UnenrollStudentAsync(int enrollmentId);
        Task<IEnumerable<Course>> GetCoursesWithEnrollmentCountAsync();
    }
}