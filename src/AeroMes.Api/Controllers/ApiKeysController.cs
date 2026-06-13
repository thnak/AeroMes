using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Auth.ApiKeys.Commands.CreateApiKey;
using AeroMes.Application.Auth.ApiKeys.Commands.RevokeApiKey;
using AeroMes.Application.Auth.ApiKeys.Commands.RotateApiKey;
using AeroMes.Application.Auth.ApiKeys.Queries.ListApiKeys;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/api-keys")]
[Authorize]
public class ApiKeysController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ApiKeyRead)]
    [ProducesResponseType<IReadOnlyList<ApiKeyListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await queryMediator.QueryAsync(new ListApiKeysQuery(ownerId), null, ct);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.ApiKeyCreate)]
    [ProducesResponseType<CreateApiKeyResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyRequest req, CancellationToken ct)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await commandMediator.SendAsync(
            new CreateApiKeyCommand(req.KeyName, req.AssignedRole, ownerId, req.WorkCenterId, req.ExpiresAt, req.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), result.Value!);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ApiKeyRevoke)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(int id, CancellationToken ct)
    {
        var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await commandMediator.SendAsync(new RevokeApiKeyCommand(id, actorId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/rotate")]
    [RequirePermission(Permissions.ApiKeyCreate)]
    [ProducesResponseType<RotateApiKeyResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rotate(int id, CancellationToken ct)
    {
        var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await commandMediator.SendAsync(new RotateApiKeyCommand(id, actorId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }
}

public record CreateApiKeyRequest(
    string KeyName,
    string AssignedRole,
    int? WorkCenterId,
    DateTime? ExpiresAt,
    string? Notes);
