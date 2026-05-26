using System.ComponentModel.DataAnnotations;

namespace BankApp.Api.Models
{
    public class SetThemeViewModel
    {
        [Required]
        public string ThemeName { get; set; }
    }
}
