using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportJobStatus;

public record GetImportJobStatusQuery(int ImportJobId) : IQuery<ImportJobSummaryDto?>;
