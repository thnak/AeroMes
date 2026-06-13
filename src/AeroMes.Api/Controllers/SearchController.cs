using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Search;
using AeroMes.Application.Search.Commands.FullReindex;
using AeroMes.Application.Search.Queries.GlobalSearch;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/search")]
[Authorize]
public sealed class SearchController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SearchResultPageDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] string[]? types,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new SearchResultPageDto(q ?? "", 0, page, pageSize, []));

        var permissions = User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value);

        var result = await queryMediator.QueryAsync(
            new GlobalSearchQuery(q, types, page, pageSize, permissions), null, ct);
        return Ok(result);
    }

    [HttpPost("reindex")]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reindex(
        [FromQuery] string? indexName = null,
        CancellationToken ct = default)
    {
        var result = await commandMediator.SendAsync(new FullReindexCommand(indexName), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }
}
