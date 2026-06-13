using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.PrintPackagingLabel;

public record PrintPackagingLabelCommand(int PackagingOrderID, string? LabelTemplate = null)
    : ICommand<ValidationResult<int>>;
