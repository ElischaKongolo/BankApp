using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterAsync(User user, string password);
        Task<User> AuthenticateAsync(string email, string password);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<bool> UpdateAsync(User user);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
