using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Maintenance.Commands.StartPmWorkOrder;

public record StartPmWorkOrderCommand(int MwoId) : ICommand<ValidationResult<Unit>>;

public class StartPmWorkOrderHandler(IMaintenancePlanRepository repo)
    : ICommandHandler<StartPmWorkOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(StartPmWorkOrderCommand cmd, CancellationToken ct)
    {
        var mwo = await repo.GetWorkOrderByIdAsync(cmd.MwoId, ct);
        if (mwo is null)
            return ValidationResult<Unit>.NotFound($"Lệnh bảo trì #{cmd.MwoId} không tồn tại.");

        try
        {
            mwo.Start();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
