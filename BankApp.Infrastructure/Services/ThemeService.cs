using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankApp.Core.Models;
using BankApp.Core.Interfaces;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    public class ThemeService : IThemeService
    {
        private readonly BankDbContext _context;

        public ThemeService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<UserTheme> GetUserThemeAsync(int userId)
        {
            var userTheme = await _context.UserThemes
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (userTheme == null)
            {
                // Create default theme for user
                userTheme = new UserTheme
                {
                    UserId = userId,
                    ThemeName = "Light",
                    PrimaryColor = PredefinedThemes.Light.PrimaryColor,
                    SecondaryColor = PredefinedThemes.Light.SecondaryColor,
                    BackgroundColor = PredefinedThemes.Light.BackgroundColor,
                    SurfaceColor = PredefinedThemes.Light.SurfaceColor,
                    TextColor = PredefinedThemes.Light.TextColor,
                    TextMutedColor = PredefinedThemes.Light.TextMutedColor,
                    IsDarkMode = PredefinedThemes.Light.IsDarkMode,
                    IsCustom = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserThemes.Add(userTheme);
                await _context.SaveChangesAsync();
            }

            return userTheme;
        }

        public async Task<UserTheme> SetUserThemeAsync(int userId, string themeName)
        {
            var predefinedTheme = GetThemeByNameAsync(themeName);
            if (predefinedTheme == null)
            {
                throw new ArgumentException($"Theme '{themeName}' not found");
            }

            var userTheme = await GetUserThemeAsync(userId);
            
            userTheme.ThemeName = predefinedTheme.Result.ThemeName;
            userTheme.PrimaryColor = predefinedTheme.Result.PrimaryColor;
            userTheme.SecondaryColor = predefinedTheme.Result.SecondaryColor;
            userTheme.BackgroundColor = predefinedTheme.Result.BackgroundColor;
            userTheme.SurfaceColor = predefinedTheme.Result.SurfaceColor;
            userTheme.TextColor = predefinedTheme.Result.TextColor;
            userTheme.TextMutedColor = predefinedTheme.Result.TextMutedColor;
            userTheme.IsDarkMode = predefinedTheme.Result.IsDarkMode;
            userTheme.IsCustom = false;
            userTheme.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return userTheme;
        }

        public async Task<UserTheme> CreateCustomThemeAsync(UserTheme theme)
        {
            theme.IsCustom = true;
            theme.CreatedAt = DateTime.UtcNow;
            theme.UpdatedAt = DateTime.UtcNow;

            _context.UserThemes.Add(theme);
            await _context.SaveChangesAsync();
            return theme;
        }

        public async Task<UserTheme> UpdateCustomThemeAsync(UserTheme theme)
        {
            if (!theme.IsCustom)
            {
                throw new InvalidOperationException("Can only update custom themes");
            }

            theme.UpdatedAt = DateTime.UtcNow;
            _context.UserThemes.Update(theme);
            await _context.SaveChangesAsync();
            return theme;
        }

        public async Task<IEnumerable<UserTheme>> GetAvailableThemesAsync()
        {
            var predefinedThemes = new List<UserTheme>
            {
                PredefinedThemes.Light,
                PredefinedThemes.Dark,
                PredefinedThemes.Ocean,
                PredefinedThemes.Forest,
                PredefinedThemes.Sunset,
                PredefinedThemes.Midnight
            };

            return predefinedThemes;
        }

        public async Task<UserTheme> GetThemeByNameAsync(string themeName)
        {
            var predefinedThemes = await GetAvailableThemesAsync();
            return predefinedThemes.FirstOrDefault(t => t.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task DeleteCustomThemeAsync(int themeId, int userId)
        {
            var theme = await _context.UserThemes
                .FirstOrDefaultAsync(t => t.Id == themeId && t.UserId == userId && t.IsCustom);

            if (theme != null)
            {
                _context.UserThemes.Remove(theme);
                await _context.SaveChangesAsync();
            }
        }
    }
}
