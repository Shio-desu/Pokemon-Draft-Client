using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits.Interfaces;

public interface ICircuitUserHandlerService
{
    ConcurrentDictionary<string, UserCircuits> Users { get; }
    event EventHandler UsersChanged;
    void Connect(string user, string circuitId);
    void Disconnect(string user, string circuitId);
}