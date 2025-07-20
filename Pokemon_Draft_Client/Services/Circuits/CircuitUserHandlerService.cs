using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public static class ConnectionStates
{
    public const string Connected = "Connected";
    public const string Disconnected =  "Disconnected";
}

public class CircuitUserHandlerService : ICircuitUserHandlerService
{
    public ConcurrentDictionary<string, string> UserConnectionStatesMap { get; private set; } = new();
    public static event EventHandler<string>? UserCircuitsChanged;
    void OnUserCircuitsChanged(string username) => UserCircuitsChanged?.Invoke(this, username);
    
    public CircuitUserHandlerService()
    {
        CircuitHandlerService.UsernameChanged += HandleUsernameChanged;
    }
    
    public void Connect(string username, string circuitId)
    {
        // checks if user was already logged from a different session
        if (UserConnectionStatesMap.ContainsKey(username))
            return;
        
        UserConnectionStatesMap[username] = ConnectionStates.Connected;
        
    }

    public void Disconnect(string username, string circuitId)
    {
        // removes the circuit id from the user circuit-list
        if (!UserConnectionStatesMap[username].CircuitIds.Remove(circuitId)) 
            return;
        if (UserConnectionStatesMap[username].CircuitIds.Count != 0) 
            return;
        
        // removes the user from the user-dictionary if they don't have a connected circuit anymore
        UserConnectionStatesMap.TryRemove(username, out var userRemoved);
        if (userRemoved != null)
        {
            OnUserCircuitsChanged(username);
        }
    }

    private void HandleUsernameChanged(object? sender, UsernameEventArgs usernames)
    {
        UserConnectionStatesMap.Remove(usernames.OldUsername, out var removedUserCircuits);
        if (removedUserCircuits != null) 
            UserConnectionStatesMap[usernames.NewUsername] = removedUserCircuits;
        OnUserCircuitsChanged(usernames.OldUsername);
    }

    private void HandleHeartbeatOmitted(object? sender, string username)
    {
        
    }
}