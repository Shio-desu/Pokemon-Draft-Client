using System.Security.Claims;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class CircuitHandlerService(IHttpContextAccessor httpContextAccessor, ICircuitUserHandlerService circuitUserHandlerService) : CircuitHandler
{
    private string CircuitId { get; set; } = "";
    private string Username { get; set; } = "";
    
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
}