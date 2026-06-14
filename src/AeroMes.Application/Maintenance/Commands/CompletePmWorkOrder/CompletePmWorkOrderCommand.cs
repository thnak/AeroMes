using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.CompletePmWorkOrder;

public record ChecklistResultInput(int ItemId, bool IsCompleted, string? Notes);

public record CompletePmWorkOrderCommand(
    int MwoId,
    string CompletedBy,
    IReadOnlyList<ChecklistResultInput> ChecklistResults) : ICommand<ValidationResult<Unit>>;

public class CompletePmWorkOrderHandler(IMaintenancePlanRepository repo)
    : ICommandHandler<CompletePmWorkOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CompletePmWorkOrderCommand cmd, CancellationToken ct)
    {
        var mwo = await repo.GetWorkOrderByIdAsync(cmd.MwoId, ct);
        if (mwo is null)
            return ValidationResult<Unit>.NotFound($"Lệnh bảo trì #{cmd.MwoId} không tồn tại.");

        try
        {
            foreach (var r in cmd.ChecklistResults)
            {
                var result = MaintenanceChecklistResult.Create(
                    cmd.MwoId, r.ItemId, r.IsCompleted, r.Notes, cmd.CompletedBy);
                await repo.AddChecklistResultAsync(result, ct);
            }

            mwo.Complete();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
