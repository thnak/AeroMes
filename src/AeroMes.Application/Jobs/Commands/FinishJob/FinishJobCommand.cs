using MediatR;

namespace AeroMes.Application.Jobs.Commands.FinishJob;

public record FinishJobCommand(long JobId, DateTime? EndTime = null) : IRequest<FinishJobResult>;

public record FinishJobResult(long JobID, string Status, DateTime EndTime);
