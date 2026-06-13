using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.DeleteInspectionRequest;

public record DeleteInspectionRequestCommand(int RequestID, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
