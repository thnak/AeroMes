using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CloseBundleOperation;

public record CloseBundleOperationCommand(
    string BundleBarcode,
    int QtyOK,
    int QtyNG,
    string? DefectCodes = null) : ICommand<ValidationResult<Unit>>;
