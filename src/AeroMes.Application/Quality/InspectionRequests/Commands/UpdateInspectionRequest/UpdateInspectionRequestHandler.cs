using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.UpdateInspectionRequest;

public class UpdateInspectionRequestHandler(IQualityInspectionRequestRepository repository)
    : ICommandHandler<UpdateInspectionRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateInspectionRequestCommand command, CancellationToken ct)
    {
        var request = await repository.GetByIdAsync(command.RequestID, ct);
        if (request is null) return ValidationResult<Unit>.NotFound($"Phiếu yêu cầu #{command.RequestID} không tồn tại.");

        try
        {
            request.Update(command.RequestDate, command.InspectionPurpose, command.RequesterName,
                command.RequestingDepartment, command.RecipientPerson, command.RecipientDepartment,
                command.InspectionDeadline, command.InspectionQuantity, command.Priority,
                command.Description, command.ProductionOrderId, command.StatisticalSheetId,
                command.InspectionSubject, command.SubcontractingOrderId, command.ProductId,
                command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
