using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.PackSerials;

public class PackSerialsHandler(
    ISerialUnitRepository repo,
    IValidator<PackSerialsCommand> validator) : ICommandHandler<PackSerialsCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(PackSerialsCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        try
        {
            var units = await repo.GetBySerialNumbersAsync(cmd.SerialNumbers, ct);
            if (units.Count != cmd.SerialNumbers.Count)
                return ValidationResult<int>.Failure("One or more serial numbers not found.");

            foreach (var unit in units)
            {
                if (unit.Status is SerialUnitStatus.Scrapped or SerialUnitStatus.Recalled)
                    return ValidationResult<int>.Failure($"Serial {unit.SerialNumber} cannot be packed in status {unit.Status}.");

                var agg = SerialAggregation.Pack(unit.SerialID, cmd.CaseSSCC);
                await repo.AddAggregationAsync(agg, ct);

                var ev = SerialEvent.Log(SerialEventType.Packed, unit.SerialID,
                    payload: cmd.CaseSSCC, operatorCode: cmd.OperatorCode);
                await repo.AddEventAsync(ev, ct);
            }

            // If pallet SSCC provided, nest the case under the pallet
            if (!string.IsNullOrWhiteSpace(cmd.PalletSSCC))
            {
                var palletAgg = SerialAggregation.PackSSCC(cmd.CaseSSCC, cmd.PalletSSCC);
                await repo.AddAggregationAsync(palletAgg, ct);
            }

            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(units.Count);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
