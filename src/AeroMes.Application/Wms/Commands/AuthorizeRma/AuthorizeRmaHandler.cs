using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AuthorizeRma;

public class AuthorizeRmaHandler(
    IRmaRepository rmaRepo,
    IUnitOfWork uow)
    : ICommandHandler<AuthorizeRmaCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        AuthorizeRmaCommand cmd, CancellationToken ct)
    {
        try
        {
            var rma = await rmaRepo.GetByIdWithLinesAsync(cmd.RmaId, ct);
            if (rma is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy RMA #{cmd.RmaId}.");

            rma.Authorize(cmd.AuthorizedBy ?? "system");
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
