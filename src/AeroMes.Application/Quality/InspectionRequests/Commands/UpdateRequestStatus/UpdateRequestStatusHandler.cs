using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.UpdateRequestStatus;

public class UpdateRequestStatusHandler(IQualityInspectionRequestRepository repository)
    : ICommandHandler<UpdateRequestStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateRequestStatusCommand command, CancellationToken ct)
    {
        var request = await repository.GetByIdAsync(command.RequestID, ct);
        if (request is null) return ValidationResult<Unit>.NotFound($"Phiếu yêu cầu #{command.RequestID} không tồn tại.");

        request.SetStatus(command.Status, command.UpdatedBy);
        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
