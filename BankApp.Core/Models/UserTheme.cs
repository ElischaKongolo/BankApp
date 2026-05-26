using System;

namespace BankApp.Core.Models
{
    public class UserTheme
    {
        public int Id { get; set; }
        public string ThemeName { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string BackgroundColor { get; set; }
        public string SurfaceColor { get; set; }
        public string TextColor { get; set; }
        public string TextMutedColor { get; set; }
        public bool IsDarkMode { get; set; }
        public bool IsCustom { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public static class PredefinedThemes
    {
        public static readonly UserTheme Light = new UserTheme
        {
            ThemeName = "Light",
            PrimaryColor = "#4318FF",
            SecondaryColor = "#868CFF",
            BackgroundColor = "#f4f7fe",
            SurfaceColor = "#ffffff",
            TextColor = "#2b3674",
            TextMutedColor = "#a3aed1",
            IsDarkMode = false,
            IsCustom = false
        };

        public static readonly UserTheme Dark = new UserTheme
        {
            ThemeName = "Dark",
            PrimaryColor = "#818CF8",
            SecondaryColor = "#A5B4FC",
            BackgroundColor = "#0f172a",
            SurfaceColor = "#1e293b",
            TextColor = "#f1f5f9",
            TextMutedColor = "#94a3b8",
            IsDarkMode = true,
            IsCustom = false
        };

        public static readonly UserTheme Ocean = new UserTheme
        {
            ThemeName = "Ocean",
            PrimaryColor = "#0891B2",
            SecondaryColor = "#06B6D4",
            BackgroundColor = "#F0F9FF",
            SurfaceColor = "#FFFFFF",
            TextColor = "#0C4A6E",
            TextMutedColor = "#64748B",
            IsDarkMode = false,
            IsCustom = false
        };

        public static readonly UserTheme Forest = new UserTheme
        {
            ThemeName = "Forest",
            PrimaryColor = "#059669",
            SecondaryColor = "#10B981",
            BackgroundColor = "#F0FDF4",
            SurfaceColor = "#FFFFFF",
            TextColor = "#064E3B",
            TextMutedColor = "#6B7280",
            IsDarkMode = false,
            IsCustom = false
        };

        public static readonly UserTheme Sunset = new UserTheme
        {
            ThemeName = "Sunset",
            PrimaryColor = "#DC2626",
            SecondaryColor = "#F97316",
            BackgroundColor = "#FFF7ED",
            SurfaceColor = "#FFFFFF",
            TextColor = "#7C2D12",
            TextMutedColor = "#9CA3AF",
            IsDarkMode = false,
            IsCustom = false
        };

        public static readonly UserTheme Midnight = new UserTheme
        {
            ThemeName = "Midnight",
            PrimaryColor = "#7C3AED",
            SecondaryColor = "#A78BFA",
            BackgroundColor = "#1E1B4B",
            SurfaceColor = "#312E81",
            TextColor = "#EDE9FE",
            TextMutedColor = "#C4B5FD",
            IsDarkMode = true,
            IsCustom = false
        };
    }
}
