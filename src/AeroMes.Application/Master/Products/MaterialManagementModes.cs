using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Products;

/// <summary>
/// Company-wide material tracking models (SystemOptions.MaterialManagementType).
/// Variant and specification sub-records are only writable under their
/// respective modes — see issue #72.
/// </summary>
public static class MaterialManagementModes
{
    public const string None = "None";
    public const string VariantCode = "VariantCode";
    public const string SpecificationCode = "SpecificationCode";

    public static async Task EnsureModeAsync(
        this ISystemOptionsRepository optionsRepo, string requiredMode, CancellationToken ct)
    {
        var options = await optionsRepo.GetAsync(ct);
        if (!string.Equals(options.MaterialManagementType, requiredMode, StringComparison.OrdinalIgnoreCase))
            throw new DomainException(
                $"Thao tác này yêu cầu chế độ quản lý vật tư '{requiredMode}' — hệ thống đang ở chế độ '{options.MaterialManagementType}'. " +
                "Thay đổi tại Settings → System Options.");
    }
}
