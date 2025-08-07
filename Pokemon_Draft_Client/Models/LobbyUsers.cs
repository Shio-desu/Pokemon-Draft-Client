using System.Collections.Concurrent;
using System.Security.Claims;
using DataAccessLibrary.Models;

namespace Pokemon_Draft_Client.Models;

public class User
{
    public required string Username { get; set; }
    public bool IsConnected { get; set; }
    public bool IsReady { get; set; }
    public bool IsOwner { get; set; }
}

public class LobbyUsers
{
    public required SessionModel Session { get; set; }
    public ConcurrentDictionary<string, User> UsersDict { get; set; } = [];
}

