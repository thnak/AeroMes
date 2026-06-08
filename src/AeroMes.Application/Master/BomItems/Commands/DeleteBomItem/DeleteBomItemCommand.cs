using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;

public record DeleteBomItemCommand(int BomId) : IRequest;
