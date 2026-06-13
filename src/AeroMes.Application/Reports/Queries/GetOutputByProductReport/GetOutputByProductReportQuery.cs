using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOutputByProductReport;

public record GetOutputByProductReportQuery(
    DateTime From,
    DateTime To) : IQuery<IReadOnlyList<ProductOutputDto>>;
