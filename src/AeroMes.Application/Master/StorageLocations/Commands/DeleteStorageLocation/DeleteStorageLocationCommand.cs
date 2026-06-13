using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public record DeleteStorageLocationCommand(int Id, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
