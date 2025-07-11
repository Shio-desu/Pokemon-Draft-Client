using System.Security.Claims;
using DataAccessLibrary.Interfaces;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class CircuitHandlerService(IHttpContextAccessor httpContextAccessor, ICircuitUserHandlerService circuitUserHandlerService, IUserData userData) : CircuitHandler
{
    private string CircuitId { get; set; } = "";
    private int UserId { get; set; }
    
    // On opening an instance of this web app, the circuit id and corresponding user is saved
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitId = circuit.Id;
        var username = httpContextAccessor.HttpContext?.User.Identity?.Name ?? string.Empty;
        if (username == string.Empty)
            UserId = -1;
        else
        {
            var users = userData.GetUsers();
            var user = users.Result.Find(user => user.Username == username);
            if (user != null)
                UserId = user.UserId;
            else
                UserId = -1;
        }
        circuitUserHandlerService.Connect(UserId, CircuitId);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    // On closing an instance of this web app
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitUserHandlerService.Disconnect(UserId, CircuitId);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}