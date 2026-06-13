using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.ScrapMold;

public record ScrapMoldCommand(string MoldCode, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
