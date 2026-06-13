using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFinishedProductIntakeRequest;

public record DeleteFinishedProductIntakeRequestCommand(int IntakeRequestId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
