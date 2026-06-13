using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReceiveBundleAtStation;

public class ReceiveBundleAtStationHandler(
    IBundleRepository bundleRepo,
    IOperationRepository operationRepo,
    IValidator<ReceiveBundleAtStationCommand> validator)
    : ICommandHandler<ReceiveBundleAtStationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ReceiveBundleAtStationCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var bundle = await bundleRepo.GetByBarcodeAsync(cmd.BundleBarcode, ct);
        if (bundle is null)
            return ValidationResult<Unit>.NotFound($"Bundle '{cmd.BundleBarcode}' not found.");

        var openMovement = await bundleRepo.GetOpenMovementAsync(bundle.BundleID, ct);
        if (openMovement is not null)
            return ValidationResult<Unit>.Failure(
                $"Bundle '{cmd.BundleBarcode}' is already in progress at operation '{openMovement.OperationCode}'. Close the current operation first.");

        var operation = await operationRepo.GetByCodeAsync(cmd.OperationCode, ct);
        var samMinutes = operation?.SAM_Minutes;

        var movement = BundleMovement.Open(
            bundle.BundleID, cmd.OperationCode, cmd.WorkCenterID, cmd.OperatorID, samMinutes);
        bundle.ReceiveAtStation(cmd.OperationCode, cmd.WorkCenterID);

        await bundleRepo.AddMovementAsync(movement, ct);
        await bundleRepo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
