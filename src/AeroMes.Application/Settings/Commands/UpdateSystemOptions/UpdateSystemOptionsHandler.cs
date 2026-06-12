using AeroMes.Application.Interfaces;
using AeroMes.Application.Master.Products;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Settings.Commands.UpdateSystemOptions;

public class UpdateSystemOptionsHandler(
    ISystemOptionsRepository repo,
    IProductRepository productRepo,
    IUnitOfWork uow)
    : ICommandHandler<UpdateSystemOptionsCommand>
{
    public async Task HandleAsync(UpdateSystemOptionsCommand cmd, CancellationToken ct)
    {
        var options = await repo.GetAsync(ct);
        await EnsureManagementTypeChangeAllowedAsync(
            options.MaterialManagementType, cmd.Options.MaterialManagementType, ct);
        options.Update(cmd.Options, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }

    // Switching the material tracking model with live data would orphan
    // variant links / specification codes — block until they are removed.
    private async Task EnsureManagementTypeChangeAllowedAsync(string current, string requested, CancellationToken ct)
    {
        if (string.Equals(current, requested, StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(current, MaterialManagementModes.VariantCode, StringComparison.OrdinalIgnoreCase)
            && await productRepo.AnyVariantLinksAsync(ct))
            throw new DomainException(
                "Không thể đổi chế độ quản lý vật tư khi còn sản phẩm biến thể — gỡ liên kết biến thể trước.");

        if (string.Equals(current, MaterialManagementModes.SpecificationCode, StringComparison.OrdinalIgnoreCase)
            && await productRepo.AnySpecificationsAsync(ct))
            throw new DomainException(
                "Không thể đổi chế độ quản lý vật tư khi còn mã quy cách — xóa các mã quy cách trước.");
    }
}
