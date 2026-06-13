using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.DeleteCharacteristic;

public record DeleteCharacteristicCommand(int CharId) : ICommand<ValidationResult<Unit>>;
