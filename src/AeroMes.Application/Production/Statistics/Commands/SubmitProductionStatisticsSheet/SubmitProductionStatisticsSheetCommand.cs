using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Statistics.Commands.SubmitProductionStatisticsSheet;

public sealed record SubmitProductionStatisticsSheetCommand(int SheetId)
    : ICommand<ValidationResult<Unit>>;
