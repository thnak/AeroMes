using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEcoFromEcr;

public record CreateEcoFromEcrCommand(
    string EcrNumber,
    string NewEcNumber,
    string? RequestedBy) : ICommand<ValidationResult<string>>;
