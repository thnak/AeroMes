using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecallFinishedProductIntakeRequest;

public record RecallFinishedProductIntakeRequestCommand(int IntakeRequestId, string? RecalledBy)
    : ICommand<ValidationResult<Unit>>;
