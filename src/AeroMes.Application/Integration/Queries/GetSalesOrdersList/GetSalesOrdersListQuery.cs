using AeroMes.Application.Common;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrdersList;

public record GetSalesOrdersListQuery(
    string? SoCode,
    string? Status,
    bool IncludeUnconfirmed,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<SalesOrderSummaryDto>>;
