using StudentManagementSystem.Models;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<bool> RegisterUserAsync(User user, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        string GenerateJwtToken(User user);
        Task<bool> ValidateTokenAsync(string token);
        int? GetUserIdFromToken(string token);
        UserRole? GetUserRoleFromToken(string token);
    }
}