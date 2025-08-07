using System.Security.Claims;

namespace Pokemon_Draft_Client.Models;

public class UserCircuits
{
    public string Username { get; set; }
    public List<string> CircuitIds { get; set; } = [];
}