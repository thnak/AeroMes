using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Import.Commands.ExecuteImport;

public record ExecuteImportCommand(int ImportJobId) : ICommand<ValidationResult<int>>;
