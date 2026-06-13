using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.UpdateInspectionRequest;

public record UpdateInspectionRequestCommand(
    int RequestID,
    DateOnly RequestDate,
    InspectionRequestPurpose InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    string? RecipientDepartment,
    DateTime InspectionDeadline,
    decimal? InspectionQuantity,
    InspectionPriority? Priority,
    string? Description,
    int? ProductionOrderId,
    int? StatisticalSheetId,
    string? InspectionSubject,
    int? SubcontractingOrderId,
    int? ProductId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
