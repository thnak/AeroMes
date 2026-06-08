using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.CreateBomItem;

public record CreateBomItemCommand(
    string ParentProductCode,
    string ChildProductCode,
    decimal RequiredQty,
    decimal ScrapFactor,
    string? CreatedBy) : IRequest<int>;
