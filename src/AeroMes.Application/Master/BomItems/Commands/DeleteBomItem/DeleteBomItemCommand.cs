using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;

public record DeleteBomItemCommand(int BomId, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
