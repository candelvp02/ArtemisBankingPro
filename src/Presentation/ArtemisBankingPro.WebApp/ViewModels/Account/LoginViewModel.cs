using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.WebApp.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "User name is required.")]
    [Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}