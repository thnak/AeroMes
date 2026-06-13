using AeroMes.Domain.Labeling;

namespace AeroMes.Application.Labeling.Services;

public interface IIdentityEncodingService
{
    string EncodeMaterial(string materialId, string? supplierId, string? lotNumber);
    string EncodeProduct(string productCode, string? workOrderId, string? lotNumber);
    string EncodeLocation(string warehouseId, string? rowId, string? binId);
    string EncodeWorkstation(string workCenterId, string? machineId);

    bool TryParse(string payload, out LabelPayload? result);
}
