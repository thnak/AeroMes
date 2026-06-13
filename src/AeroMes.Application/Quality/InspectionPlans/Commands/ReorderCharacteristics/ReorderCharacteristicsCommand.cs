using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.ReorderCharacteristics;

public record ReorderCharacteristicsCommand(int PlanId, List<int> CharIds) : ICommand<ValidationResult<Unit>>;
