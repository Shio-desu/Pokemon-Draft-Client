using System.Collections.Concurrent;
using System.Security.Claims;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits.Interfaces;

public interface ICircuitUserHandlerService
{
    ConcurrentDictionary<string, UserCircuits> UserCircuitsMap { get; }
    static event EventHandler? UserCircuitsChanged;
    void Connect(string username, string circuitId);
    void Disconnect(string username, string circuitId);
}