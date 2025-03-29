using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course> GetByCourseCodeAsync(string courseCode);
        Task<IEnumerable<Course>> GetActiveCourses();
        Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId);
        Task<IEnumerable<Course>> GetCoursesBySemesterAsync(string semester);
        Task<IEnumerable<Course>> GetCoursesWithEnrollmentCountAsync();
    }

    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Course> GetByCourseCodeAsync(string courseCode)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
        }

        public async Task<IEnumerable<Course>> GetActiveCourses()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId)
        {
            return await _dbSet.Where(c => c.FacultyId == facultyId).ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesBySemesterAsync(string semester)
        {
            return await _dbSet.Where(c => c.Semester == semester && c.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesWithEnrollmentCountAsync()
        {
            return await _dbSet
                .Select(c => new Course
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    Title = c.Title,
                    Description = c.Description,
                    Credits = c.Credits,
                    MaxStudents = c.MaxStudents,
                    FacultyId = c.FacultyId,
                    Semester = c.Semester,
                    IsActive = c.IsActive,
                    Enrollments = c.Enrollments
                })
                .ToListAsync();
        }
    }
}