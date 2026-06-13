using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability;

public class SerialLotLineage : Entity
{
    public long ID { get; private set; }
    public Guid SerialID { get; private set; }
    public string ComponentLotNumber { get; private set; } = string.Empty;
    public string ComponentProductCode { get; private set; } = string.Empty;
    public decimal? QuantityUsed { get; private set; }
    public string? UoM { get; private set; }
    public int? RoutingStepID { get; private set; }
    public DateTime AssembledAt { get; private set; }

    private SerialLotLineage() { }

    public static SerialLotLineage Create(
        Guid serialId,
        string componentLotNumber,
        string componentProductCode,
        decimal? quantityUsed,
        string? uom,
        int? routingStepId)
    {
        return new SerialLotLineage
        {
            SerialID = serialId,
            ComponentLotNumber = componentLotNumber.Trim().ToUpperInvariant(),
            ComponentProductCode = componentProductCode.Trim().ToUpperInvariant(),
            QuantityUsed = quantityUsed,
            UoM = uom?.Trim(),
            RoutingStepID = routingStepId,
            AssembledAt = DateTime.UtcNow,
        };
    }
}
