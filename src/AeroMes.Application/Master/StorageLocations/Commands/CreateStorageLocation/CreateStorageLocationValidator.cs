using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;

public class CreateStorageLocationValidator : AbstractValidator<CreateStorageLocationCommand>
{
    public CreateStorageLocationValidator(IStorageLocationRepository locationRepo, IWorkCenterRepository wcRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code must be at most 20 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await locationRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Location code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.LocationType)
            .IsInEnum().WithMessage("Invalid location type.");

        RuleFor(x => x.WorkCenterId)
            .NotNull().WithMessage("WIP location must specify a WorkCenter.")
            .GreaterThan(0).WithMessage("WorkCenter id must be positive.")
            .MustAsync(async (id, ct) => await wcRepo.ExistsAsync(id!.Value, ct))
            .WithMessage(x => $"WorkCenter {x.WorkCenterId} does not exist.")
            .When(x => x.LocationType == LocationType.Wip);
    }
}
