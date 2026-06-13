namespace AeroMes.Application.Traceability.Services;

public enum SerialStrategy { GS1_SGTIN, PREFIX_DATE_SEQ, UDI_HRI }

public record SerialGenerationResult(
    string SerialNumber,
    string? GTIN,
    string? UDI);

public interface ISerialNumberGenerationService
{
    Task<IReadOnlyList<SerialGenerationResult>> GenerateAsync(
        string productCode,
        string lotNumber,
        int quantity,
        SerialStrategy strategy,
        string? gtin = null,
        CancellationToken ct = default);
}
