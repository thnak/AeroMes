using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GenerateRecallAuditReport;

public sealed record GenerateRecallAuditReportQuery(Guid RecallID)
    : IQuery<RecallAuditReportDto?>;
