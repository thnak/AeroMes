using AeroMes.Domain.Production.Repositories;
using FluentValidation;

namespace AeroMes.Application.Jobs.Commands.FinishJob;

public class FinishJobValidator : AbstractValidator<FinishJobCommand>
{
    public FinishJobValidator(IJobRepository repo)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("Job id is required.")
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"Job {x.JobId} does not exist.");
    }
}
