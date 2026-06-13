using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Services;

/// <summary>
/// Calls the external ERP HTTP API using settings stored in SystemOptions.
/// The ERP is expected to expose:
///   GET {base}/api/sales-orders?since={iso8601}
///   GET {base}/api/production-orders?since={iso8601}
///   GET {base}/api/health
/// </summary>
public class HttpErpClient(AppDbContext db, IHttpClientFactory httpFactory, ILogger<HttpErpClient> logger)
    : IErpClient
{
    public async Task<IReadOnlyList<ErpSalesOrderRecord>> GetSalesOrdersAsync(DateTime? since, CancellationToken ct)
    {
        var (client, baseUrl) = await CreateClientAsync(ct);
        var url = $"{baseUrl}/api/sales-orders" + (since.HasValue ? $"?since={since:O}" : string.Empty);

        logger.LogInformation("Fetching SO from ERP: {Url}", url);
        var response = await client.GetFromJsonAsync<List<ErpSoPayload>>(url, ct) ?? [];
        return [.. response.Select(p => new ErpSalesOrderRecord(
            p.SoCode, p.OrderDate, p.CustomerName, p.DeliveryDate, p.CustomerCode))];
    }

    public async Task<IReadOnlyList<ErpProductionOrderRecord>> GetProductionOrdersAsync(DateTime? since, CancellationToken ct)
    {
        var (client, baseUrl) = await CreateClientAsync(ct);
        var url = $"{baseUrl}/api/production-orders" + (since.HasValue ? $"?since={since:O}" : string.Empty);

        logger.LogInformation("Fetching PO from ERP: {Url}", url);
        var response = await client.GetFromJsonAsync<List<ErpPoPayload>>(url, ct) ?? [];
        return [.. response.Select(p => new ErpProductionOrderRecord(
            p.PoCode, p.ProductCode, p.TargetQuantity, p.SoCode, p.PlannedStart, p.PlannedEnd))];
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        var (client, baseUrl) = await CreateClientAsync(ct);
        var response = await client.GetAsync($"{baseUrl}/api/health", ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<(HttpClient client, string baseUrl)> CreateClientAsync(CancellationToken ct)
    {
        var options = await db.Set<AeroMes.Domain.Settings.SystemOptions>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (options is null || string.IsNullOrWhiteSpace(options.ErpBaseUrl))
            throw new InvalidOperationException("ERP base URL is not configured.");

        var client = httpFactory.CreateClient("erp");
        if (!string.IsNullOrWhiteSpace(options.ErpApiKey))
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ErpApiKey);

        return (client, options.ErpBaseUrl.TrimEnd('/'));
    }

    // ─── Payload shapes from the ERP ─────────────────────────────────────────

    private sealed class ErpSoPayload
    {
        [JsonPropertyName("soCode")]   public string SoCode { get; init; } = string.Empty;
        [JsonPropertyName("orderDate")] public DateTime OrderDate { get; init; }
        [JsonPropertyName("customerName")] public string? CustomerName { get; init; }
        [JsonPropertyName("deliveryDate")] public DateTime? DeliveryDate { get; init; }
        [JsonPropertyName("customerCode")] public string? CustomerCode { get; init; }
    }

    private sealed class ErpPoPayload
    {
        [JsonPropertyName("poCode")]        public string PoCode { get; init; } = string.Empty;
        [JsonPropertyName("productCode")]   public string ProductCode { get; init; } = string.Empty;
        [JsonPropertyName("targetQuantity")] public int TargetQuantity { get; init; }
        [JsonPropertyName("soCode")]        public string? SoCode { get; init; }
        [JsonPropertyName("plannedStart")]  public DateTime? PlannedStart { get; init; }
        [JsonPropertyName("plannedEnd")]    public DateTime? PlannedEnd { get; init; }
    }
}
