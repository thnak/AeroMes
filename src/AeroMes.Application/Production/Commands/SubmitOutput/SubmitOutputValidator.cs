using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;

namespace AeroMes.Application.Production.Commands.SubmitOutput;

public class SubmitOutputValidator : AbstractValidator<SubmitOutputCommand>
{
    public SubmitOutputValidator(IJobRepository jobRepo, IDefectCodeRepository defectRepo)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("Job id is required.")
            .MustAsync(async (id, ct) => await jobRepo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"Job {x.JobId} does not exist.");

        RuleFor(x => x.QtyOk)
            .GreaterThanOrEqualTo(0).WithMessage("QtyOk cannot be negative.");

        RuleFor(x => x.QtyNg)
            .GreaterThanOrEqualTo(0).WithMessage("QtyNg cannot be negative.");

        RuleFor(x => x)
            .Must(x => x.QtyOk + x.QtyNg > 0)
            .WithMessage("At least one unit (OK or NG) must be submitted.");

        RuleFor(x => x.Defects)
            .Must(d => d.All(e => e.Qty > 0))
            .WithMessage("Each defect entry must have a positive quantity.")
            .When(x => x.Defects is { Count: > 0 });

        RuleForEach(x => x.Defects)
            .ChildRules(d =>
            {
                d.RuleFor(x => x.DefectCode).NotEmpty().WithMessage("DefectCode is required.");
                d.RuleFor(x => x.Qty).GreaterThan(0).WithMessage("Defect quantity must be positive.");
            })
            .When(x => x.Defects is { Count: > 0 });
    }
}
