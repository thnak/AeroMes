using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOrderProgressReport;

public record GetOrderProgressReportQuery(
    DateTime? From,
    DateTime? To,
    string? Status) : IQuery<IReadOnlyList<OrderProgressDto>>;
