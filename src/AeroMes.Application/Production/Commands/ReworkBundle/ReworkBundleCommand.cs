using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReworkBundle;

public record ReworkBundleCommand(
    int BundleID,
    string TargetOperationCode,
    string Reason) : ICommand<ValidationResult<Unit>>;
