using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SendFinishedProductIntakeRequest;

public class SendFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IUnitOfWork uow)
    : ICommandHandler<SendFinishedProductIntakeRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SendFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var request = await intakeRepo.GetByIdWithLinesAsync(cmd.IntakeRequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu nhập '{cmd.IntakeRequestId}' không tồn tại.");

        try
        {
            request.Send(cmd.SentBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
