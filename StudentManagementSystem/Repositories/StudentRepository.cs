using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Repositories
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student> GetByStudentNumberAsync(string studentNumber);
        Task<Student> GetStudentWithUserAsync(int id);
        Task<IEnumerable<Student>> GetStudentsWithUsersAsync();
        Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId);
    }

    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Student> GetByStudentNumberAsync(string studentNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<Student> GetStudentWithUserAsync(int id)
        {
            return await _dbSet.Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Student>> GetStudentsWithUsersAsync()
        {
            return await _dbSet.Include(s => s.User).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId)
        {
            return await _dbSet.Where(s => s.Enrollments.Any(e => e.CourseId == courseId))
                .Include(s => s.User)
                .ToListAsync();
        }
    }
}