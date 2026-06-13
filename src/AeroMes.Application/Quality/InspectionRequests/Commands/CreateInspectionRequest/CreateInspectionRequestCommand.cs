using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.CreateInspectionRequest;

public record CreateInspectionRequestCommand(
    string RequestNumber,
    DateOnly RequestDate,
    InspectionRequestPurpose InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    DateTime InspectionDeadline,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
