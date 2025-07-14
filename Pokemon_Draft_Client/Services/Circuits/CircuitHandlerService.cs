using System.Security.Claims;
using DataAccessLibrary.Interfaces;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class UserIdEventArgs(int newUserId, int oldUserId) : EventArgs
{
    public int NewUserId { get; private set; } = newUserId;
    public int OldUserId { get; private set; }  = oldUserId;
}

public class CircuitHandlerService(IHttpContextAccessor httpContextAccessor, ICircuitUserHandlerService circuitUserHandlerService, IUserData userData) : CircuitHandler
{
    public string CircuitId { get; private set; } = "";
    public int UserId { get; private set; } = -1;
    public static event EventHandler<UserIdEventArgs>? UserIdChanged;
    void OnUserIdChanged(int oldUserId, int newUserId) => UserIdChanged?.Invoke(this, new UserIdEventArgs(oldUserId, newUserId));
    
    // On opening an instance of this web app, the circuit id and corresponding user is saved
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitId = circuit.Id;
        var username = httpContextAccessor.HttpContext?.User.Identity?.Name ?? string.Empty;
        
        var users = userData.GetUsers();
        var user = users.Result.Find(user => user.Username == username);
        if (user != null)
            UserId = user.UserId;
        
        circuitUserHandlerService.Connect(UserId, CircuitId);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    // On closing an instance of this web app
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitUserHandlerService.Disconnect(UserId, CircuitId);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public void ChangeUserId(int newUserId)
    {
        OnUserIdChanged(UserId, newUserId);
        UserId = newUserId;
    }
}