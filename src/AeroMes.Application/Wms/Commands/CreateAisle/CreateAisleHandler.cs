using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateAisle;

public class CreateAisleHandler(
    IAisleRepository repo,
    IWarehouseZoneRepository zoneRepo,
    IUnitOfWork uow,
    IValidator<CreateAisleCommand> validator)
    : ICommandHandler<CreateAisleCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateAisleCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var zone = await zoneRepo.GetByIdAsync(cmd.ZoneId, ct);
        if (zone is null)
            return ValidationResult<int>.NotFound($"Khu vực '{cmd.ZoneId}' không tồn tại.");

        if (await repo.CodeExistsInZoneAsync(cmd.ZoneId, cmd.AisleCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["AisleCode"] = [$"Mã lối đi '{cmd.AisleCode}' đã tồn tại trong khu vực này."]
            });

        var entity = Aisle.Create(cmd.ZoneId, cmd.AisleCode, cmd.PickSequence, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(entity.AisleId);
    }
}
