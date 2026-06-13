using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.RecordLotLineage;

public sealed class RecordLotLineageHandler(ILotTraceabilityRepository repo)
    : ICommandHandler<RecordLotLineageCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RecordLotLineageCommand cmd, CancellationToken ct)
    {
        try
        {
            var edge = LotLineage.Record(
                cmd.ParentLotNumber, cmd.ChildLotNumber, cmd.LineageType,
                cmd.WorkOrderID, cmd.RoutingStepID, cmd.QuantityConsumed, cmd.UoM);
            await repo.AddLineageAsync(edge, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
