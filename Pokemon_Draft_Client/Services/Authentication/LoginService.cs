using System.Security.Claims;
using DataAccessLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Pokemon_Draft_Client.Services.Authentication.Interfaces;

namespace Pokemon_Draft_Client.Services.Authentication;

public struct AuthenticationResult
{
    public bool LoginSuccessful;
    public string? ErrorMessage;
    public string Username;
}

public class LoginService(IUserData userData, IHttpContextAccessor httpContextAccessor) : ILoginService
{
    public async Task<AuthenticationResult> Authenticate(string? username, string? password)
    {
        var result = new AuthenticationResult
        {
            LoginSuccessful = false
        };

        if (username == null)
        {
            result.ErrorMessage = "Please enter a Username.";
            return result;
        }
        
        if (password == null)
        {
            result.ErrorMessage = "Please enter a Password.";
            return result;
        }

        result.Username = username;
        
        var userList = await userData.GetUserByName(username);
        if (userList.Count == 0)
        {
            result.ErrorMessage = "No Account found with this Username.";
            return result;
        }
        var userAccount = userList[0];
        
        bool verify = BCrypt.Net.BCrypt.Verify(password, userAccount.Passhash);

        if (!verify)
        {
            result.ErrorMessage = "Invalid Password.";
            return result;
        }
        
        var claims = new List<Claim>()
        {
            new(ClaimTypes.Name, userAccount.Username),
            new(ClaimTypes.Role, userAccount.IsAdmin? "Administrator" : "User")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        if (httpContextAccessor.HttpContext == null)
        {
            result.ErrorMessage = "HttpContext is null";
            return result;
        }
        
        await httpContextAccessor.HttpContext.SignInAsync(principal);
        
        result.LoginSuccessful = true;
        return result;
    }
}