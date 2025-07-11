using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits.Interfaces;

public interface ICircuitUserHandlerService
{
    ConcurrentDictionary<int, UserCircuits> UserCircuitsMap { get; }
    event EventHandler UserCircuitsChanged;
    void Connect(int userId, string circuitId);
    void Disconnect(int userId, string circuitId);
}