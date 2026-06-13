using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldAssignmentHistory;

public record GetMoldAssignmentHistoryQuery(
    string MoldCode,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IQuery<IReadOnlyList<MoldAssignmentDto>>;
