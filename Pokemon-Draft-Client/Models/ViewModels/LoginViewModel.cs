using System.ComponentModel.DataAnnotations;

namespace Pokemon;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Username")]
    public required string Username { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a Password")]
    public required string Password { get; set; }
}