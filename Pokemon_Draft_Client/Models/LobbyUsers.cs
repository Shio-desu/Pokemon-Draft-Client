using System.Collections.Concurrent;
using System.Security.Claims;
using DataAccessLibrary.Models;

namespace Pokemon_Draft_Client.Models;

public class LobbyUsers
{
    public required SessionModel Session { get; set; }
    public ConcurrentDictionary<string, string> Users { get; set; } = [];
}