using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ApproveRegrindUsage;

public class ApproveRegrindUsageHandler(
    IMaterialBlendLogRepository repo,
    IValidator<ApproveRegrindUsageCommand> validator) : ICommandHandler<ApproveRegrindUsageCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ApproveRegrindUsageCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var blendLog = await repo.GetByIdAsync(cmd.BlendLogID, ct);
        if (blendLog is null)
            return ValidationResult<Unit>.NotFound($"Blend log {cmd.BlendLogID} not found.");

        try
        {
            blendLog.Approve(cmd.ApprovedBy, cmd.ApprovalNotes);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
