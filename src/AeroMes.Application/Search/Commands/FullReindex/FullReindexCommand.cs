using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Search.Commands.FullReindex;

public record FullReindexCommand(string? IndexName = null)
    : ICommand<ValidationResult<int>>;
