using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.UnpackSerials;

public class UnpackSerialsHandler(
    ISerialUnitRepository repo,
    IValidator<UnpackSerialsCommand> validator) : ICommandHandler<UnpackSerialsCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(UnpackSerialsCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        var aggregations = await repo.GetActiveAggregationsBySSCCAsync(cmd.SSCC, ct);
        if (aggregations.Count == 0)
            return ValidationResult<int>.Failure($"No active aggregations found for SSCC {cmd.SSCC}.");

        foreach (var agg in aggregations)
        {
            agg.Disaggregate();

            if (agg.ChildSerialID.HasValue)
            {
                var ev = SerialEvent.Log(SerialEventType.Unpacked, agg.ChildSerialID.Value,
                    payload: cmd.SSCC, operatorCode: cmd.OperatorCode);
                await repo.AddEventAsync(ev, ct);
            }
        }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(aggregations.Count);
    }
}
