using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CloseBundleOperation;

public class CloseBundleOperationHandler(
    IBundleRepository bundleRepo,
    IValidator<CloseBundleOperationCommand> validator)
    : ICommandHandler<CloseBundleOperationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CloseBundleOperationCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var bundle = await bundleRepo.GetByBarcodeAsync(cmd.BundleBarcode, ct);
        if (bundle is null)
            return ValidationResult<Unit>.NotFound($"Bundle '{cmd.BundleBarcode}' not found.");

        var movement = await bundleRepo.GetOpenMovementAsync(bundle.BundleID, ct);
        if (movement is null)
            return ValidationResult<Unit>.Failure($"Bundle '{cmd.BundleBarcode}' has no open movement to close.");

        try
        {
            movement.Close(cmd.QtyOK, cmd.QtyNG, cmd.DefectCodes);
            bundle.AdvanceStatus(movement.OperationCode, movement.WorkCenterID, cmd.QtyOK, cmd.QtyNG);
            await bundleRepo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
