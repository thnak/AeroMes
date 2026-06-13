using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ApplyRecallQuarantine;

public sealed record ApplyRecallQuarantineCommand(Guid RecallID, string AppliedBy)
    : ICommand<ValidationResult<RecallQuarantineResultDto>>;
