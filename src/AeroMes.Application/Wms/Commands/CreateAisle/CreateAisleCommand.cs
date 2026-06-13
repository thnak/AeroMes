using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateAisle;

public record CreateAisleCommand(int ZoneId, string AisleCode, int PickSequence, string? CreatedBy)
    : ICommand<ValidationResult<int>>;
