using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolsDueForCalibration;

public class GetToolsDueForCalibrationHandler(IToolRepository repo)
    : IQueryHandler<GetToolsDueForCalibrationQuery, IReadOnlyList<ToolCalibrationDueDto>>
{
    public async Task<IReadOnlyList<ToolCalibrationDueDto>> HandleAsync(
        GetToolsDueForCalibrationQuery query, CancellationToken ct)
    {
        var tools = await repo.GetDueForCalibrationAsync(query.WithinDays, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return tools
            .Select(t => new ToolCalibrationDueDto(
                t.ToolId, t.ToolCode, t.ToolName, t.Status.ToString(),
                t.LastCalibratedAt, t.NextCalibrationDue!.Value,
                t.NextCalibrationDue.Value.DayNumber - today.DayNumber,
                t.NextCalibrationDue.Value <= today))
            .ToList();
    }
}
