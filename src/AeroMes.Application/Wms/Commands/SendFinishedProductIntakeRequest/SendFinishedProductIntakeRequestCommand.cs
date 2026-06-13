using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SendFinishedProductIntakeRequest;

public record SendFinishedProductIntakeRequestCommand(int IntakeRequestId, string? SentBy)
    : ICommand<ValidationResult<Unit>>;
