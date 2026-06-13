using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecallFinishedProductIntakeRequest;

public class RecallFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IUnitOfWork uow)
    : ICommandHandler<RecallFinishedProductIntakeRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        RecallFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var request = await intakeRepo.GetByIdAsync(cmd.IntakeRequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu nhập '{cmd.IntakeRequestId}' không tồn tại.");

        try
        {
            request.Recall(cmd.RecalledBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
