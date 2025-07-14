using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class CircuitUserHandlerService : ICircuitUserHandlerService
{
    public ConcurrentDictionary<string, UserCircuits> UserCircuitsMap { get; private set; } = new();
    public static event EventHandler<string>? UserCircuitsChanged;
    void OnUserCircuitsChanged(string username) => UserCircuitsChanged?.Invoke(this, username);
    
    public CircuitUserHandlerService()
    {
        CircuitHandlerService.UsernameChanged += HandleUsernameChanged;
    }
    
    public void Connect(string username, string circuitId)
    {
        // checks if user was already logged from a different session and adds the circuit to the user
        if (UserCircuitsMap.ContainsKey(username))
        {
            UserCircuitsMap[username].CircuitIds.Add(circuitId);
        }
        else // or creates a new user circuit collection, if user wasn't logged before
        {
            var userCircuits = new UserCircuits
            {
                Username = username,
                CircuitIds = [circuitId]
            };
            UserCircuitsMap[username] = userCircuits;
        }
    }

    public void Disconnect(string username, string circuitId)
    {
        // removes the circuit id from the user circuit-list
        if (!UserCircuitsMap[username].CircuitIds.Remove(circuitId)) 
            return;
        if (UserCircuitsMap[username].CircuitIds.Count != 0) 
            return;
        
        // removes the user from the user-dictionary if they don't have a connected circuit anymore
        UserCircuitsMap.TryRemove(username, out var userRemoved);
        if (userRemoved != null)
        {
            OnUserCircuitsChanged(username);
        }
    }

    private void HandleUsernameChanged(object? sender, UsernameEventArgs usernames)
    {
        UserCircuitsMap.Remove(usernames.OldUsername, out var removedUserCircuits);
        if (removedUserCircuits != null) 
            UserCircuitsMap[usernames.NewUsername] = removedUserCircuits;
        OnUserCircuitsChanged(usernames.OldUsername);
    }
}