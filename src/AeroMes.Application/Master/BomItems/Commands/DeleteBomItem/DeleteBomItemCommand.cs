using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;

public record DeleteBomItemCommand(int BomId) : ICommand;
