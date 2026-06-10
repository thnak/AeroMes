using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.UpsertMachineProductConfig;

public class UpsertMachineProductConfigHandler(
    IMachineProductConfigRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpsertMachineProductConfigCommand>
{
    public async Task HandleAsync(UpsertMachineProductConfigCommand cmd, CancellationToken ct)
    {
        var existing = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, ct);
        if (existing is not null)
        {
            existing.Update(cmd.IdealCycleTimeSeconds, cmd.TargetThroughputPerHour,
                cmd.SetupTimeSeconds, cmd.EffectiveFrom);
        }
        else
        {
            var entity = MachineProductConfig.Create(
                cmd.MachineCode, cmd.ProductCode,
                cmd.IdealCycleTimeSeconds, cmd.TargetThroughputPerHour,
                cmd.SetupTimeSeconds, cmd.EffectiveFrom, cmd.RoutingStepId);
            await repo.AddAsync(entity, ct);
        }
        await uow.SaveChangesAsync(ct);
    }
}
