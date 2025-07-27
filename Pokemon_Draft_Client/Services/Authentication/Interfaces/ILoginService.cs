namespace Pokemon_Draft_Client.Services.Authentication.Interfaces;

public interface ILoginService
{
    Task<AuthenticationResult> Authenticate(string? username, string? password);
}