using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateZone;

public class CreateZoneHandler(
    IWarehouseZoneRepository repo,
    IUnitOfWork uow,
    IValidator<CreateZoneCommand> validator)
    : ICommandHandler<CreateZoneCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateZoneCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        if (await repo.CodeExistsAsync(cmd.ZoneCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["ZoneCode"] = [$"Mã khu vực '{cmd.ZoneCode}' đã tồn tại."]
            });

        var entity = WarehouseZone.Create(
            cmd.ZoneCode, cmd.ZoneName, cmd.ZoneType,
            cmd.StorageLocationId, cmd.WarehouseId, cmd.TemperatureZone, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(entity.ZoneId);
    }
}
