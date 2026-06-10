using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using DomainRules = AeroMes.Domain.Master.WorkOrderAutoRules;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.UpsertWorkOrderAutoRules;

public class UpsertWorkOrderAutoRulesHandler(
    IWorkOrderAutoRulesRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpsertWorkOrderAutoRulesCommand, int>
{
    public async Task<int> HandleAsync(UpsertWorkOrderAutoRulesCommand cmd, CancellationToken ct)
    {
        DomainRules entity;

        var existing = cmd.WorkCenterId.HasValue
            ? await repo.GetByWorkCenterAsync(cmd.WorkCenterId.Value, ct)
            : await repo.GetFactoryWideAsync(ct);

        if (existing is not null)
        {
            existing.UpdateDetails(cmd.AutoStartEnabled, cmd.AutoCompleteOnTargetReached,
                cmd.RequireDeleteConfirmToken, cmd.MaxConcurrentJobs, cmd.UpdatedBy);
            entity = existing;
        }
        else
        {
            entity = DomainRules.Create(
                cmd.WorkCenterId, cmd.AutoStartEnabled, cmd.AutoCompleteOnTargetReached,
                cmd.RequireDeleteConfirmToken, cmd.MaxConcurrentJobs, cmd.UpdatedBy);
            await repo.AddAsync(entity, ct);
        }
        await uow.SaveChangesAsync(ct);
        return entity.RuleId;
    }
}
