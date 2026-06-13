using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Traceability.Repositories;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Services;

public class SerialNumberGenerationService(
    ISerialUnitRepository repo,
    ILogger<SerialNumberGenerationService> logger) : ISerialNumberGenerationService
{
    public async Task<IReadOnlyList<SerialGenerationResult>> GenerateAsync(
        string productCode,
        string lotNumber,
        int quantity,
        SerialStrategy strategy,
        string? gtin = null,
        CancellationToken ct = default)
    {
        int existingCount = await repo.GetSerialCountForLotAsync(lotNumber, ct);
        var results = new List<SerialGenerationResult>(quantity);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStr = today.ToString("yyyyMMdd");

        for (int i = 0; i < quantity; i++)
        {
            int seq = existingCount + i + 1;
            string serial;
            string? udi = null;

            switch (strategy)
            {
                case SerialStrategy.GS1_SGTIN:
                    // GTIN-14 + serial component (zero-padded 6 digits)
                    var gtinPart = (gtin ?? "00000000000000").PadLeft(14, '0')[..14];
                    serial = $"(01){gtinPart}(21){seq:D6}";
                    break;

                case SerialStrategy.UDI_HRI:
                    // FDA UDI Human Readable Interpretation: (01)GTIN(11)ProductionDate(21)Serial
                    var udiGtin = (gtin ?? "00000000000000").PadLeft(14, '0')[..14];
                    var serialPart = $"{productCode[..Math.Min(productCode.Length, 6)]}{seq:D4}";
                    udi = $"(01){udiGtin}(11){todayStr}(21){serialPart}";
                    serial = $"{productCode.ToUpperInvariant()}-{todayStr}-{seq:D6}";
                    break;

                case SerialStrategy.PREFIX_DATE_SEQ:
                default:
                    var prefix = productCode.Length > 6 ? productCode[..6] : productCode;
                    serial = $"{prefix.ToUpperInvariant()}-{todayStr}-{seq:D4}";
                    break;
            }

            results.Add(new SerialGenerationResult(serial, gtin, udi));
            logger.LogDebug("Generated serial {Serial} for lot {Lot} strategy {Strategy}", serial, lotNumber, strategy);
        }

        return results;
    }
}
