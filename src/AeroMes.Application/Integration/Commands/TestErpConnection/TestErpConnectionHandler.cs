using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.TestErpConnection;

public class TestErpConnectionHandler(
    IErpClient erpClient,
    ISystemOptionsRepository optionsRepo)
    : ICommandHandler<TestErpConnectionCommand, ValidationResult<bool>>
{
    public async Task<ValidationResult<bool>> HandleAsync(
        TestErpConnectionCommand cmd, CancellationToken ct)
    {
        var options = await optionsRepo.GetAsync(ct);
        if (string.IsNullOrWhiteSpace(options.ErpBaseUrl))
            return ValidationResult<bool>.Failure("ERP base URL is not configured.");

        try
        {
            var ok = await erpClient.TestConnectionAsync(ct);
            return ok
                ? ValidationResult<bool>.Ok(true)
                : ValidationResult<bool>.Failure("ERP returned an unexpected response.");
        }
        catch (Exception ex)
        {
            return ValidationResult<bool>.Failure($"Connection failed: {ex.Message}");
        }
    }
}
