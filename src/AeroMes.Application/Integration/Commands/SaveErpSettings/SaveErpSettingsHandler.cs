using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SaveErpSettings;

public class SaveErpSettingsHandler(
    ISystemOptionsRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<SaveErpSettingsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SaveErpSettingsCommand cmd, CancellationToken ct)
    {
        var options = await repo.GetAsync(ct);
        options.UpdateErpSettings(cmd.ErpEnabled, cmd.ErpBaseUrl, cmd.ErpApiKey, cmd.ErpSyncIntervalMinutes);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
