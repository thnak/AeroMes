using AeroMes.Domain.Wms.Repositories;
using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateGrn;

public class CreateGrnValidator : AbstractValidator<CreateGrnCommand>
{
    public CreateGrnValidator(IGoodsReceiptNoteRepository grnRepo)
    {
        RuleFor(x => x.GrnCode)
            .NotEmpty().MaximumLength(50)
            .MustAsync(async (code, ct) => !await grnRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"GRN code '{x.GrnCode}' already exists.");

        RuleFor(x => x.StorageLocationId).GreaterThan(0);
        RuleFor(x => x.ReceivedBy).NotEmpty().MaximumLength(50);
    }
}
