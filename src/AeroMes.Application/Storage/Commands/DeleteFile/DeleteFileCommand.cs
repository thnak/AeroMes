using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Storage.Commands.DeleteFile;

public sealed record DeleteFileCommand(Guid Id, string DeletedBy) : ICommand<ValidationResult<Unit>>;
