using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.DeleteInspectionRequest;

public class DeleteInspectionRequestHandler(IQualityInspectionRequestRepository repository)
    : ICommandHandler<DeleteInspectionRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteInspectionRequestCommand command, CancellationToken ct)
    {
        var request = await repository.GetByIdAsync(command.RequestID, ct);
        if (request is null) return ValidationResult<Unit>.NotFound($"Phiếu yêu cầu #{command.RequestID} không tồn tại.");

        request.SoftDelete(command.DeletedBy);
        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
