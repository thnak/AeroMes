using AeroMes.Application.Labeling.Services;
using AeroMes.Domain.Labeling;

namespace AeroMes.Infrastructure.Labeling;

public sealed class IdentityEncodingService : IIdentityEncodingService
{
    public string EncodeMaterial(string materialId, string? supplierId, string? lotNumber)
        => Build(LabelContentType.Material, materialId, supplierId ?? "", lotNumber ?? "");

    public string EncodeProduct(string productCode, string? workOrderId, string? lotNumber)
        => Build(LabelContentType.Product, productCode, workOrderId ?? "", lotNumber ?? "");

    public string EncodeLocation(string warehouseId, string? rowId, string? binId)
        => Build(LabelContentType.Location, warehouseId, rowId ?? "", binId ?? "");

    public string EncodeWorkstation(string workCenterId, string? machineId)
        => Build(LabelContentType.Workstation, workCenterId, machineId ?? "");

    public bool TryParse(string payload, out LabelPayload? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(payload)) return false;

        // Format: {VERSION}_{CONTENT_TYPE}_{DATA_BODY}
        var firstUnderscore  = payload.IndexOf('_');
        if (firstUnderscore < 0) return false;

        var secondUnderscore = payload.IndexOf('_', firstUnderscore + 1);
        if (secondUnderscore < 0) return false;

        var version     = payload[..firstUnderscore];
        var contentType = payload[(firstUnderscore + 1)..secondUnderscore];
        var body        = payload[(secondUnderscore + 1)..];

        // Forward-compatible: unknown trailing fields are preserved
        var fields = body.Split('|');

        result = new LabelPayload(version, contentType, fields);
        return true;
    }

    private static string Build(string contentType, params string[] fields)
        => $"{LabelSchemaVersion.V1}_{contentType}_{string.Join("|", fields)}";
}
