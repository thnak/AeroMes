using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToJob;

public class AssignMoldToJobHandler(
    IMoldRepository moldRepo,
    IMoldCompatibilityRepository compatibilityRepo,
    IJobRepository jobRepo,
    IMoldAssignmentRepository assignmentRepo,
    IValidator<AssignMoldToJobCommand> validator) : ICommandHandler<AssignMoldToJobCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AssignMoldToJobCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        try
        {
            var mold = await moldRepo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null) return ValidationResult<Unit>.NotFound($"Mold '{cmd.MoldCode}' not found.");

            if (mold.Status == MoldStatus.Scrapped)
                return ValidationResult<Unit>.Failure($"Mold '{cmd.MoldCode}' is scrapped and cannot be assigned.");
            if (mold.Status == MoldStatus.InMaintenance)
                return ValidationResult<Unit>.Failure($"Mold '{cmd.MoldCode}' is under maintenance.");

            var isCompatible = await compatibilityRepo.IsCompatibleAsync(cmd.MoldCode, cmd.MachineCode, ct);
            if (!isCompatible)
                throw new MoldMachineIncompatibleException(cmd.MoldCode, cmd.MachineCode);

            // Check for active mount
            var existingMount = await assignmentRepo.GetActiveMountAsync(cmd.MoldCode, ct);
            if (existingMount != null)
                return ValidationResult<Unit>.Failure($"Mold '{cmd.MoldCode}' is already mounted (AssignmentID {existingMount.AssignmentID}). Unmount first.");

            var job = await jobRepo.GetByIdAsync(cmd.JobID, ct);
            if (job is null) return ValidationResult<Unit>.NotFound($"Job {cmd.JobID} not found.");

            job.AssignMold(cmd.MoldCode);

            var assignment = MoldAssignment.Create(cmd.MoldCode, cmd.MachineCode, cmd.WOID, cmd.AssignedBy);
            await assignmentRepo.AddAsync(assignment, ct);

            await assignmentRepo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (MoldMachineIncompatibleException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
