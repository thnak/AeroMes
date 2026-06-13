using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFinishedProductIntakeRequest;

public class DeleteFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteFinishedProductIntakeRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var request = await intakeRepo.GetByIdAsync(cmd.IntakeRequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu nhập '{cmd.IntakeRequestId}' không tồn tại.");

        if (request.Status != IntakeRequestStatus.Draft)
            return ValidationResult<Unit>.Failure(
                $"Chỉ có thể xóa yêu cầu nhập ở trạng thái Nháp. Trạng thái hiện tại: {request.Status}.");

        try
        {
            request.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
