using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordDisassemblyRecovery;

public class RecordDisassemblyRecoveryHandler(IDisassemblyOrderRepository repo)
    : ICommandHandler<RecordDisassemblyRecoveryCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RecordDisassemblyRecoveryCommand cmd, CancellationToken ct)
    {
        if (cmd.ActualQty < 0) return ValidationResult<Unit>.Invalid(new Dictionary<string, string[]>
            { ["ActualQty"] = ["ActualQty must be >= 0."] });

        var order = await repo.GetByIdAsync(cmd.DisassemblyOrderID, ct);
        if (order is null) return ValidationResult<Unit>.NotFound($"DisassemblyOrder '{cmd.DisassemblyOrderID}' not found.");

        try
        {
            order.RecordActualQty(cmd.ProductCode, cmd.ActualQty);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
