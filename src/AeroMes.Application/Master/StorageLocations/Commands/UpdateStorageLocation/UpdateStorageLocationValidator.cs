using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;

public class UpdateStorageLocationValidator : AbstractValidator<UpdateStorageLocationCommand>
{
    public UpdateStorageLocationValidator(IStorageLocationRepository locationRepo, IWorkCenterRepository wcRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("StorageLocation id is required.")
            .MustAsync(async (id, ct) => await locationRepo.ExistsAsync(id, ct))
            .WithMessage(x => $"StorageLocation {x.Id} does not exist.");

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
