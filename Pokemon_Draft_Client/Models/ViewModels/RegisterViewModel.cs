using System.ComponentModel.DataAnnotations;

namespace Pokemon;

public class RegisterViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Username")]
    public string? Username { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Password")]
    public string? Password { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter the Password again")]
    public string? RepeatPassword { get; set; }
}