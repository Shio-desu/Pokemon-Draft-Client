namespace Pokemon_Draft_Client;

public class CircuitHandlerService : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var circuitId = circuit.Id;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var circuitId = circuit.Id;
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}