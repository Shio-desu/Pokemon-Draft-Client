using System.Security.Claims;

namespace Pokemon_Draft_Client.Models;

public class UserCircuits
{
    public int UserId { get; set; }
    public List<string> CircuitIds { get; set; } = [];
}