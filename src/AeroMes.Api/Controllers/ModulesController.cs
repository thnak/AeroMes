using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/modules")]
[Authorize]
public class ModulesController(IQueryMediator queryMediator, IPermissionService permissionService) : ControllerBase
{
    // Module → permission codes: user needs at least one to see the module.
    // Empty array = visible to all authenticated users.
    private static readonly Dictionary<string, string[]> ModulePermissions = new()
    {
        ["production"]   = [Permissions.WorkOrderRead, Permissions.ProductionRead],
        ["quality"]      = [Permissions.QualityRead, Permissions.NcrRead],
        ["warehouse"]    = [Permissions.WarehouseRead],
        ["maintenance"]  = [Permissions.MaintenanceRead],
        ["reports"]      = [Permissions.ReportRead],
        ["master"]       = [Permissions.MasterDataRead],
        ["planning"]     = [Permissions.MasterDataRead],
        ["admin"]        = [Permissions.UserRead, Permissions.PermissionManage],
        ["iot"]          = [],
        ["lab"]          = [],
        ["traceability"] = [Permissions.InventoryRead],
        ["costing"]      = [],
    };

    [HttpGet("status")]
    [ProducesResponseType<ApiResponse<ModuleStatusResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ModuleStatusResponse>>> GetStatus(CancellationToken ct)
    {
        var allowedIds = await GetAllowedModuleIdsAsync(ct);
        var result = await queryMediator.QueryAsync(new GetModuleStatusQuery(allowedIds), null, ct);
        return Ok(new ApiResponse<ModuleStatusResponse>(true, "OK", result));
    }

    [HttpGet("status/{id}")]
    [ProducesResponseType<ApiResponse<ModuleStatusDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ModuleStatusDto>>> GetStatusById(string id, CancellationToken ct)
    {
        var allowedIds = await GetAllowedModuleIdsAsync(ct);

        if (!allowedIds.Contains(id))
            return Forbid();

        var snapshot = await queryMediator.QueryAsync(new GetModuleStatusQuery(allowedIds), null, ct);
        var module = snapshot.Modules.FirstOrDefault(m => m.Id == id);

        if (module is null)
            return NotFound();

        return Ok(new ApiResponse<ModuleStatusDto>(true, "OK", module));
    }

    private async Task<IReadOnlySet<string>> GetAllowedModuleIdsAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        if (userId is null)
            return new HashSet<string>();

        var allowed = new HashSet<string>();

        foreach (var (moduleId, perms) in ModulePermissions)
        {
            if (perms.Length == 0)
            {
                allowed.Add(moduleId);
                continue;
            }

            foreach (var perm in perms)
            {
                if (await permissionService.HasPermissionAsync(userId, perm, ct))
                {
                    allowed.Add(moduleId);
                    break;
                }
            }
        }

        return allowed;
    }
}
