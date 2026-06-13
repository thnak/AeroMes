using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.IdentifyRecallScope;

public sealed record IdentifyRecallScopeCommand(Guid RecallID, string RequestedBy)
    : ICommand<ValidationResult<RecallScopeDto>>;
