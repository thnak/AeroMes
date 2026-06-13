using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Commands.ConfirmHandoverForm;

public sealed class ConfirmHandoverFormHandler(
    IStageHandoverRepository repo,
    IUnitOfWork uow) : ICommandHandler<ConfirmHandoverFormCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ConfirmHandoverFormCommand cmd, CancellationToken ct)
    {
        var form = await repo.GetByIdWithLinesAsync(cmd.FormId, ct);
        if (form is null) return ValidationResult<Unit>.NotFound($"Handover form #{cmd.FormId} not found.");

        try
        {
            form.Confirm(cmd.ConfirmedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
