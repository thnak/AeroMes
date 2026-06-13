using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CloseRma;

public class CloseRmaHandler(
    IRmaRepository rmaRepo,
    IUnitOfWork uow)
    : ICommandHandler<CloseRmaCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CloseRmaCommand cmd, CancellationToken ct)
    {
        try
        {
            var rma = await rmaRepo.GetByIdAsync(cmd.RmaId, ct);
            if (rma is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy RMA #{cmd.RmaId}.");

            rma.Close(cmd.ClosedBy ?? "system");
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
