using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateZone;

public class UpdateZoneHandler(
    IWarehouseZoneRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateZoneCommand> validator)
    : ICommandHandler<UpdateZoneCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateZoneCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var entity = await repo.GetByIdAsync(cmd.ZoneId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Khu vực '{cmd.ZoneId}' không tồn tại.");

        entity.UpdateDetails(cmd.ZoneName, cmd.ZoneType, cmd.TemperatureZone, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
