using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetMidSessionWIP;

public sealed record GetMidSessionWIPQuery(int? WorkOrderID, string? MachineCode)
    : IQuery<IReadOnlyList<ProcessRecordDto>>;
