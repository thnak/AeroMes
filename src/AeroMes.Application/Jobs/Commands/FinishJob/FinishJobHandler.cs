using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Jobs.Commands.FinishJob;

public class FinishJobHandler(
    IJobRepository jobRepo,
    IUnitOfWork uow,
    IValidator<FinishJobCommand> validator)
    : ICommandHandler<FinishJobCommand, ValidationResult<FinishJobResult>>
{
    public async Task<ValidationResult<FinishJobResult>> HandleAsync(FinishJobCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<FinishJobResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var job = await jobRepo.GetByIdAsync(cmd.JobId, ct);
            if (job is null) return ValidationResult<FinishJobResult>.NotFound($"Entity '{cmd.JobId}' was not found.");

            job.Finish(cmd.EndTime);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<FinishJobResult>.Ok(new FinishJobResult(job.JobID, job.Status.ToString().ToUpperInvariant(), job.EndTime!.Value));
        }        catch (DomainException ex)
        {
            return ValidationResult<FinishJobResult>.Failure(ex.Message);
        }
    }
}
