using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Jobs.Commands.FinishJob;

public record FinishJobCommand(long JobId, DateTime? EndTime = null) : ICommand<ValidationResult<FinishJobResult>>;

public record FinishJobResult(long JobID, string Status, DateTime EndTime);
