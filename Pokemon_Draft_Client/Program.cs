global using Microsoft.AspNetCore.Components.Server.Circuits;
using Blazored.Modal;
using Microsoft.AspNetCore.Authentication.Cookies;
using DataAccessLibrary;
using DataAccessLibrary.Interfaces;
using Pokemon_Draft_Client;
using Pokemon_Draft_Client.Components;
using Pokemon_Draft_Client.Services;
using Pokemon_Draft_Client.Services.Authentication;
using Pokemon_Draft_Client.Services.Authentication.Interfaces;
using Pokemon_Draft_Client.Services.Circuits;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. --------------------------------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

    // add Authentication and Authorization for login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".Draft.Authentication";
        options.Cookie.MaxAge = TimeSpan.FromMinutes(60);
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/accessdenied";
    });

builder.Services.AddBlazoredModal();

// Add Database-Services
builder.Services.AddTransient<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddTransient<IUserData, UserData>();
builder.Services.AddTransient<ISessionData, SessionData>();
builder.Services.AddTransient<IParticipationData, ParticipationData>();
builder.Services.AddTransient<IPickData, PickData>();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ICircuitUserHandlerService, CircuitUserHandlerService>();
builder.Services.AddSingleton<ILobbyUserHandlerService, LobbyUserHandlerService>();
builder.Services.AddScoped<CircuitHandler, CircuitHandlerService>();
builder.Services.AddScoped<ILoginService, LoginService>();

/*
    // add Session to the WebApp 
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // TODO: currently 10 seconds für testing purposes, tbc
    options.Cookie.Name = ".Draft.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});*/



var app = builder.Build();

// ---------------------------------------------------------------------------------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
//app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();