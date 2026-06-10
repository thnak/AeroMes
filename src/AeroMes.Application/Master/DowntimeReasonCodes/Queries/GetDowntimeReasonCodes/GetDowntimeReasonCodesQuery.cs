using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Queries.GetDowntimeReasonCodes;

public record GetDowntimeReasonCodesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<DowntimeReasonCodeDto>>;

public record DowntimeReasonCodeDto(
    string ReasonCode,
    string ReasonName,
    DowntimeCategory Category,
    int? SlaMinutes,
    bool RequiresApproval,
    bool IsActive);
