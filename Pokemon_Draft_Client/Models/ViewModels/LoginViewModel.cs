using System.ComponentModel.DataAnnotations;

namespace Pokemon_Draft_Client.Models.ViewModels;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Username")]
    public string? Username { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Password")]
    public string? Password { get; set; }
}