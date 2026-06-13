using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreatePackagingBom;

public record PackagingBomLineInput(string MaterialCode, decimal Quantity, string UnitCode, string? Notes);

public record CreatePackagingBomCommand(
    string ProductCode,
    IReadOnlyList<PackagingBomLineInput> Lines,
    string? Notes = null) : ICommand<ValidationResult<int>>;
