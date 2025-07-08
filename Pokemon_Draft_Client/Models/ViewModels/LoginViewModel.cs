using System.ComponentModel.DataAnnotations;

namespace Pokemon;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Username")]
    public string? Username { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Password")]
    public string? Password { get; set; }
}