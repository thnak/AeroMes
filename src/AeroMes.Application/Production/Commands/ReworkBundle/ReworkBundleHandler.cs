using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReworkBundle;

public class ReworkBundleHandler(
    IBundleRepository bundleRepo,
    IValidator<ReworkBundleCommand> validator)
    : ICommandHandler<ReworkBundleCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ReworkBundleCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var bundle = await bundleRepo.GetByIdAsync(cmd.BundleID, ct);
        if (bundle is null)
            return ValidationResult<Unit>.NotFound($"Bundle {cmd.BundleID} not found.");

        try
        {
            bundle.Rework(cmd.TargetOperationCode);
            await bundleRepo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
