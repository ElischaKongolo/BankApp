using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;

namespace BankApp.Core.Interfaces
{
    public interface IThemeService
    {
        Task<UserTheme> GetUserThemeAsync(int userId);
        Task<UserTheme> SetUserThemeAsync(int userId, string themeName);
        Task<UserTheme> CreateCustomThemeAsync(UserTheme theme);
        Task<UserTheme> UpdateCustomThemeAsync(UserTheme theme);
        Task<IEnumerable<UserTheme>> GetAvailableThemesAsync();
        Task<UserTheme> GetThemeByNameAsync(string themeName);
        Task DeleteCustomThemeAsync(int themeId, int userId);
    }
}
