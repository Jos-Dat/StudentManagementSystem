using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.Repositories;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ICourseRepository> _mockCourseRepo;
    private readonly Mock<IRepository<Enrollment>> _mockEnrollmentRepo;
    private readonly Mock<IAuthService> _mockAuthService;

    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _mockStudentRepo = new Mock<IStudentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockCourseRepo = new Mock<ICourseRepository>();
        _mockEnrollmentRepo = new Mock<IRepository<Enrollment>>();
        _mockAuthService = new Mock<IAuthService>();

        _service = new StudentService(
            _mockStudentRepo.Object,
            _mockUserRepo.Object,
            _mockCourseRepo.Object,
            _mockEnrollmentRepo.Object,
            _mockAuthService.Object
        );
    }

    [Fact]
    public async Task GetAllStudentsAsync_ReturnsAllStudents()
    {
        var students = new List<Student>
        {
            new Student { Id = 1, StudentNumber = "S001" },
            new Student { Id = 2, StudentNumber = "S002" }
        };
        _mockStudentRepo.Setup(r => r.GetStudentsWithUsersAsync()).ReturnsAsync(students);

        var result = await _service.GetAllStudentsAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsStudent_WhenExists()
    {
        var student = new Student { Id = 1, StudentNumber = "S001" };
        _mockStudentRepo.Setup(r => r.GetStudentWithUserAsync(1)).ReturnsAsync(student);

        var result = await _service.GetStudentByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("S001", result.StudentNumber);
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsNull_WhenNotExists()
    {
        _mockStudentRepo.Setup(r => r.GetStudentWithUserAsync(99)).ReturnsAsync((Student)null);

        var result = await _service.GetStudentByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentByStudentNumberAsync_ReturnsStudent()
    {
        var student = new Student { StudentNumber = "SV123" };
        _mockStudentRepo.Setup(r => r.GetByStudentNumberAsync("SV123")).ReturnsAsync(student);

        var result = await _service.GetStudentByStudentNumberAsync("SV123");

        Assert.NotNull(result);
        Assert.Equal("SV123", result.StudentNumber);
    }

    [Fact]
    public async Task CreateStudentAsync_ReturnsFalse_WhenStudentExists()
    {
        var student = new Student { StudentNumber = "SV001", User = new User() };
        _mockStudentRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Student, bool>>>())).ReturnsAsync(true);

        var result = await _service.CreateStudentAsync(student, "password");

        Assert.False(result);
    }

    [Fact]
    public async Task CreateStudentAsync_ReturnsFalse_WhenRegisterFails()
    {
        var student = new Student
        {
            StudentNumber = "SV002",
            User = new User { Email = "test@mail.com", FirstName = "A", LastName = "B" }
        };

        _mockStudentRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Student, bool>>>())).ReturnsAsync(false);
        _mockAuthService.Setup(a => a.RegisterUserAsync(It.IsAny<User>(), "password")).ReturnsAsync(false);

        var result = await _service.CreateStudentAsync(student, "password");

        Assert.False(result);
    }

    [Fact]
    public async Task CreateStudentAsync_ReturnsTrue_WhenSuccess()
    {
        var student = new Student
        {
            StudentNumber = "SV003",
            User = new User { Email = "mail@mail.com", FirstName = "Test", LastName = "User" }
        };

        _mockStudentRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Student, bool>>>())).ReturnsAsync(false);
        _mockAuthService.Setup(a => a.RegisterUserAsync(It.IsAny<User>(), "password")).ReturnsAsync(true);
        _mockStudentRepo.Setup(r => r.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

        var result = await _service.CreateStudentAsync(student, "password");

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateStudentAsync_ReturnsFalse_WhenStudentNotFound()
    {
        _mockStudentRepo.Setup(r => r.GetStudentWithUserAsync(99)).ReturnsAsync((Student)null);

        var result = await _service.UpdateStudentAsync(new Student { Id = 99 });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudentAsync_ReturnsFalse_WhenStudentNotFound()
    {
        _mockStudentRepo.Setup(r => r.GetStudentWithUserAsync(99)).ReturnsAsync((Student)null);

        var result = await _service.DeleteStudentAsync(99);

        Assert.False(result);
    }

    [Fact]
    public async Task GetStudentsByCourseAsync_ReturnsStudents()
    {
        var courseId = 1;
        var students = new List<Student>
        {
            new Student { Id = 1 },
            new Student { Id = 2 }
        };

        _mockStudentRepo.Setup(r => r.GetStudentsByCourseAsync(courseId)).ReturnsAsync(students);

        var result = await _service.GetStudentsByCourseAsync(courseId);

        Assert.Equal(2, result.Count());
    }
}
