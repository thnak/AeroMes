using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Reports.Queries.GetDowntimeReport;
using AeroMes.Application.Reports.Queries.GetProductionReport;
using AeroMes.Application.Reports.Queries.GetQualityReport;

namespace AeroMes.IntegrationTests.Reports;

[Collection("Integration")]
public class ReportsTests(AeroMesWebFactory factory)
{
    private static string DateParam(DateTime d) => d.ToString("yyyy-MM-ddTHH:mm:ssZ");

    [Fact]
    public async Task ProductionReport_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/v1/reports/production?from=2024-01-01&to=2024-12-31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DowntimeReport_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/v1/reports/downtime?from=2024-01-01&to=2024-12-31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QualityReport_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/v1/reports/quality?from=2024-01-01&to=2024-12-31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProductionReport_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = DateParam(new DateTime(2024, 1, 1));
        var to = DateParam(new DateTime(2024, 12, 31));
        var response = await client.GetAsync($"/api/v1/reports/production?from={from}&to={to}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductionReportDto>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task DowntimeReport_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = DateParam(new DateTime(2024, 1, 1));
        var to = DateParam(new DateTime(2024, 12, 31));
        var response = await client.GetAsync($"/api/v1/reports/downtime?from={from}&to={to}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DowntimeReportDto>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task QualityReport_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = DateParam(new DateTime(2024, 1, 1));
        var to = DateParam(new DateTime(2024, 12, 31));
        var response = await client.GetAsync($"/api/v1/reports/quality?from={from}&to={to}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<QualityReportDto>>();
        Assert.NotNull(body?.Data);
    }
}
