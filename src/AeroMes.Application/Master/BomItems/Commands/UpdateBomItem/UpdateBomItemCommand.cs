using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.UpdateBomItem;

public record UpdateBomItemCommand(
    int BomId,
    decimal RequiredQty,
    decimal ScrapFactor,
    string UpdatedBy) : IRequest;
