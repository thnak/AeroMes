using System.Text.Json;
using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.PrintPackagingLabel;

public class PrintPackagingLabelHandler(IPackagingRepository repo)
    : ICommandHandler<PrintPackagingLabelCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(PrintPackagingLabelCommand cmd, CancellationToken ct)
    {
        var order = await repo.GetOrderByIdAsync(cmd.PackagingOrderID, ct);
        if (order is null) return ValidationResult<int>.NotFound($"PackagingOrder '{cmd.PackagingOrderID}' not found.");

        var labelPayload = JsonSerializer.Serialize(new
        {
            order.PackagingOrderID,
            order.ProductCode,
            order.IdentificationCode,
            order.PlannedQty,
            PrintedAt = DateTime.UtcNow.ToString("O"),
            Template = cmd.LabelTemplate ?? "default",
        });

        var label = order.AddLabel(labelPayload);
        label.PrintedAt = DateTime.UtcNow;
        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(label.LabelID);
    }
}
