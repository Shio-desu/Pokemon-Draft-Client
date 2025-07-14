using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class CircuitUserHandlerService : ICircuitUserHandlerService
{
    public ConcurrentDictionary<int, UserCircuits> UserCircuitsMap { get; private set; } = new();
    public static event EventHandler<int>? UserCircuitsChanged;
    void OnUserCircuitsChanged(int userId) => UserCircuitsChanged?.Invoke(this, userId);
    
    public CircuitUserHandlerService()
    {
        CircuitHandlerService.UserIdChanged += HandleUserIdChanged;
    }
    
    public void Connect(int userId, string circuitId)
    {
        // checks if user was already logged from a different session and adds the circuit to the user
        if (UserCircuitsMap.ContainsKey(userId))
        {
            UserCircuitsMap[userId].CircuitIds.Add(circuitId);
        }
        else // or creates a new user circuit collection, if user wasn't logged before
        {
            var userCircuits = new UserCircuits
            {
                UserId = userId,
                CircuitIds = [circuitId]
            };
            UserCircuitsMap[userId] = userCircuits;
        }
    }

    public void Disconnect(int userId, string circuitId)
    {
        // removes the circuit id from the user circuit-list
        if (!UserCircuitsMap[userId].CircuitIds.Remove(circuitId)) 
            return;
        if (UserCircuitsMap[userId].CircuitIds.Count != 0) 
            return;
        
        // removes the user from the user-dictionary if they don't have a connected circuit anymore
        UserCircuitsMap.TryRemove(userId, out var userRemoved);
        if (userRemoved != null)
        {
            OnUserCircuitsChanged(userId);
        }
    }

    private void HandleUserIdChanged(object? sender, UserIdEventArgs userIds)
    {
        UserCircuitsMap.Remove(userIds.OldUserId, out var removedUserCircuits);
        if (removedUserCircuits != null) 
            UserCircuitsMap[userIds.NewUserId] = removedUserCircuits;
        OnUserCircuitsChanged(userIds.OldUserId);
    }
}