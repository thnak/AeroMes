using AeroMes.Application.Common;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.GenerateScheduledMaintenance;

public record GenerateScheduledMaintenanceCommand(DateTime AsOf) : ICommand<ValidationResult<int>>;

public class GenerateScheduledMaintenanceHandler(IMaintenancePlanRepository repo)
    : ICommandHandler<GenerateScheduledMaintenanceCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        GenerateScheduledMaintenanceCommand cmd, CancellationToken ct)
    {
        var due = await repo.GetDueTemplatesAsync(cmd.AsOf, ct);
        int generated = 0;

        foreach (var template in due)
        {
            var mwo = MaintenanceWorkOrder.Create(
                template.TemplateId,
                template.MachineCode,
                MwoTriggeredBy.Schedule,
                template.Priority,
                cmd.AsOf.AddHours(8));

            await repo.AddWorkOrderAsync(mwo, ct);
            template.MarkGenerated(cmd.AsOf);
            generated++;
        }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(generated);
    }
}
