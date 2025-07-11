using System.Security.Claims;
using DataAccessLibrary.Models;

namespace Pokemon_Draft_Client.Models;

public class LobbyUsers
{
    public required SessionModel Session { get; set; }
    public List<int> UserIds { get; set; } = [];
}