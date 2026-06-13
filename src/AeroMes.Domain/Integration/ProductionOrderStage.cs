using AeroMes.Domain.Common;

namespace AeroMes.Domain.Integration;

public class ProductionOrderStage : Entity
{
    public int StageId { get; private set; }
    public int POID { get; private set; }
    public int SequenceNo { get; private set; }
    public string OperationCode { get; private set; } = string.Empty;
    public string? WorkCenterCode { get; private set; }
    public bool IsCompleted { get; private set; }

    private ProductionOrderStage() { }

    internal static ProductionOrderStage Create(
        int poid, int sequenceNo, string operationCode, string? workCenterCode)
        => new()
        {
            POID = poid,
            SequenceNo = sequenceNo,
            OperationCode = operationCode.Trim().ToUpperInvariant(),
            WorkCenterCode = workCenterCode?.Trim().ToUpperInvariant(),
        };

    public void MarkCompleted() => IsCompleted = true;
}
