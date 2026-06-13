using AeroMes.Domain.Iot.Repositories;
using FluentValidation;

namespace AeroMes.Application.Iot.Signals.Commands.AddSignal;

public class AddSignalValidator : AbstractValidator<AddSignalCommand>
{
    public AddSignalValidator(IAdapterRepository adapterRepo, ISignalMappingRepository signalRepo)
    {
        RuleFor(x => x.AdapterId).GreaterThan(0)
            .MustAsync(async (id, ct) => await adapterRepo.ExistsAsync(id, ct))
            .WithMessage("Adapter not found.");

        RuleFor(x => x.TagKey).NotEmpty();
        RuleFor(x => x.SourceAddress).NotEmpty();
        RuleFor(x => x.Scale).NotEqual(0).WithMessage("Scale cannot be zero.");

        RuleFor(x => x).MustAsync(async (cmd, ct) =>
            !await signalRepo.TagKeyExistsAsync(cmd.AdapterId, cmd.TagKey, ct))
            .WithName("TagKey")
            .WithMessage("TagKey already exists for this adapter.");
    }
}
