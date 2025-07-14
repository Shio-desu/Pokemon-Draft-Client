using System.Security.Claims;
using DataAccessLibrary.Interfaces;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class UsernameEventArgs(string newUsername, string oldUsername) : EventArgs
{
    public string NewUsername { get; private set; } = newUsername;
    public string OldUsername { get; private set; }  = oldUsername;
}

public class CircuitHandlerService(IHttpContextAccessor httpContextAccessor, ICircuitUserHandlerService circuitUserHandlerService) : CircuitHandler
{
    public string CircuitId { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public static event EventHandler<UsernameEventArgs>? UsernameChanged;
    void OnUsernameChanged(string oldUsername, string newUsername) => UsernameChanged?.Invoke(this, new UsernameEventArgs(newUsername, oldUsername));

    // On opening an instance of this web app, the circuit id and corresponding user is saved
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitId = circuit.Id;
        Username = httpContextAccessor.HttpContext?.User.Identity?.Name ?? string.Empty;
        
        circuitUserHandlerService.Connect(Username, CircuitId);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    // On closing an instance of this web app
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitUserHandlerService.Disconnect(Username, CircuitId);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public void ChangeUsername(string newUsername)
    {
        OnUsernameChanged(Username, newUsername);
        Username = newUsername;
    }
}