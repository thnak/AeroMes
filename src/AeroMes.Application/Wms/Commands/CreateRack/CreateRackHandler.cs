using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateRack;

public class CreateRackHandler(
    IRackRepository repo,
    IAisleRepository aisleRepo,
    IUnitOfWork uow,
    IValidator<CreateRackCommand> validator)
    : ICommandHandler<CreateRackCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateRackCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var aisle = await aisleRepo.GetByIdAsync(cmd.AisleId, ct);
        if (aisle is null)
            return ValidationResult<int>.NotFound($"Lối đi '{cmd.AisleId}' không tồn tại.");

        if (await repo.CodeExistsInAisleAsync(cmd.AisleId, cmd.RackCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["RackCode"] = [$"Mã kệ '{cmd.RackCode}' đã tồn tại trong lối đi này."]
            });

        var entity = Rack.Create(cmd.AisleId, cmd.RackCode, cmd.MaxWeightKg, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(entity.RackId);
    }
}
