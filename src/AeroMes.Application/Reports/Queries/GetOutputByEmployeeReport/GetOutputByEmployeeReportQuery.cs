using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOutputByEmployeeReport;

public record GetOutputByEmployeeReportQuery(
    DateTime From,
    DateTime To) : IQuery<IReadOnlyList<EmployeeOutputDto>>;
