using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class CircuitUserHandlerService : ICircuitUserHandlerService
{
    public ConcurrentDictionary<string, UserCircuits> Users { get; private set; } = new();
    public event EventHandler UsersChanged;
    
    void OnUsersChanged() => UsersChanged?.Invoke(this, EventArgs.Empty);

    public void Connect(string user, string circuitId)
    {
        // checks if user was already logged from a different session and adds the circuit to the user
        if (Users.ContainsKey(user))
        {
            Users[user].CircuitIds.Add(circuitId);
        }
        else // or creates a new user circuit collection, if user wasn't logged before
        {
            var userCircuits = new UserCircuits
            {
                User = user,
                CircuitIds = [circuitId]
            };
            Users[user] = userCircuits;
        }
    }

    public void Disconnect(string user, string circuitId)
    {
        // removes the circuit id from the user circuit-list
        if (!Users[user].CircuitIds.Remove(circuitId)) return;
        if (Users[user].CircuitIds.Count != 0) return;
        
        // removes the user from the user-dictionary if they don't have a connected circuit anymore
        Users.TryRemove(user, out var userRemoved);
        if (userRemoved != null)
        {
            OnUsersChanged();
        }
    }
}