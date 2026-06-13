using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.UpdateRequestStatus;

public record UpdateRequestStatusCommand(
    int RequestID,
    InspectionRequestStatus Status,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
