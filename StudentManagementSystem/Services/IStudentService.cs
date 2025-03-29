using StudentManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(int id);
        Task<Student> GetStudentByUserIdAsync(int userId);
        Task<Student> GetStudentByStudentNumberAsync(string studentNumber);
        Task<bool> CreateStudentAsync(Student student, string password);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId);
        Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);
        Task<IEnumerable<Course>> GetAvailableCoursesForStudentAsync(int studentId);
    }
}