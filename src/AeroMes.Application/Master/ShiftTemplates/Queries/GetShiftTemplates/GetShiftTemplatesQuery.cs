using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Queries.GetShiftTemplates;

public record GetShiftTemplatesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<ShiftTemplateDto>>;

public record ShiftTemplateDto(
    string ShiftCode,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsNightShift,
    WeekDays ValidDays,
    int? WorkCenterId,
    bool IsActive);
