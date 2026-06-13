using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.IncrementMoldShotCount;

public class IncrementMoldShotCountHandler(
    IMoldRepository moldRepo,
    IMoldAssignmentRepository assignmentRepo,
    IUnitOfWork uow,
    IValidator<IncrementMoldShotCountCommand> validator) : ICommandHandler<IncrementMoldShotCountCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(IncrementMoldShotCountCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        try
        {
            var mold = await moldRepo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null) return ValidationResult<Unit>.NotFound($"Mold '{cmd.MoldCode}' not found.");

            // shots = QtyOK / CavityCount (integer division)
            long shots = cmd.QtyOK / Math.Max(mold.Cavities, 1);
            mold.AccumulateShots(shots, "system");

            var shotLog = MoldShotLog.Create(cmd.MoldCode, cmd.JobID, shots);
            await assignmentRepo.AddShotLogAsync(shotLog, ct);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
