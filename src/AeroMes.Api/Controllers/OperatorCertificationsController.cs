using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.OperatorCertifications.Commands.RecordOperatorCertification;
using AeroMes.Application.Master.OperatorCertifications.Queries.CheckOperatorEligibility;
using AeroMes.Application.Master.OperatorCertifications.Queries.GetOperatorCertifications;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/operator-certifications")]
[Authorize]
public class OperatorCertificationsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<OperatorCertificationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee([FromQuery] string employeeCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOperatorCertificationsQuery(employeeCode), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CertificationIssuedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Record([FromBody] RecordOperatorCertificationRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordOperatorCertificationCommand(
                req.EmployeeCode, req.CertificationCode,
                req.IssuedDate, req.ExpiryDate, req.IssuedBy),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByEmployee),
            new { employeeCode = req.EmployeeCode },
            new CertificationIssuedResult(result.Value!));
    }

    [HttpGet("eligibility")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<OperatorEligibilityResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEligibility(
        [FromQuery] string employeeCode,
        [FromQuery] string machineCode,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new CheckOperatorEligibilityQuery(employeeCode, machineCode), null, ct));
}

public record RecordOperatorCertificationRequest(
    string EmployeeCode,
    string CertificationCode,
    DateOnly IssuedDate,
    DateOnly? ExpiryDate,
    string? IssuedBy);

public record CertificationIssuedResult(int CertId);
