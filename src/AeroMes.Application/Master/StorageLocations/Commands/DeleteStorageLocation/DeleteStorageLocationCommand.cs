using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public record DeleteStorageLocationCommand(int Id) : ICommand;
