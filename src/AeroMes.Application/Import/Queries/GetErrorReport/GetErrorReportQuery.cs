using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetErrorReport;

public record GetErrorReportQuery(int ImportJobId) : IQuery<byte[]?>;
