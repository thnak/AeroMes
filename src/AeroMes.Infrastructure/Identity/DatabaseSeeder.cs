using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Identity;

public class DatabaseSeeder(
    AppDbContext db,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedPermissionsAsync(ct);
        await SeedRolePermissionsAsync(ct);
        await SeedDefaultAdminAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        foreach (var name in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(name))
            {
                await roleManager.CreateAsync(new IdentityRole(name));
                logger.LogInformation("Created role {Role}", name);
            }
        }
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existing = await db.Permissions
            .Select(p => p.PermissionCode)
            .ToHashSetAsync(ct);

        var toAdd = AllPermissions()
            .Where(p => !existing.Contains(p.PermissionCode))
            .ToList();

        if (toAdd.Count > 0)
        {
            db.Permissions.AddRange(toAdd);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} permissions", toAdd.Count);
        }
    }

    private async Task SeedRolePermissionsAsync(CancellationToken ct)
    {
        var permissions = await db.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.PermissionCode, p => p.PermissionId, ct);

        var roles = await db.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Name!, r => r.Id, ct);

        var existingPairs = await db.RolePermissions
            .AsNoTracking()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(ct);

        var existingSet = existingPairs
            .Select(x => (x.RoleId, x.PermissionId))
            .ToHashSet();

        var matrix = BuildRolePermissionMatrix();
        var toAdd = new List<RolePermission>();

        foreach (var (roleName, permCodes) in matrix)
        {
            if (!roles.TryGetValue(roleName, out var roleId)) continue;

            foreach (var code in permCodes)
            {
                if (!permissions.TryGetValue(code, out var permId)) continue;
                if (!existingSet.Contains((roleId, permId)))
                    toAdd.Add(RolePermission.Create(roleId, permId));
            }
        }

        if (toAdd.Count > 0)
        {
            db.RolePermissions.AddRange(toAdd);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} role-permission assignments", toAdd.Count);
        }
    }

    private async Task SeedDefaultAdminAsync(CancellationToken ct)
    {
        var adminEmail = configuration["Seeding:AdminEmail"] ?? "system@aeromes.local";
        var adminPassword = configuration["Seeding:AdminPassword"] ?? "ChangeMe123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true,
            ForcePasswordChange = true,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create default admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, AppRoles.SystemAdmin);
        logger.LogInformation("Created default admin account {Email}", adminEmail);
    }

    // ── Permission definitions ────────────────────────────────────────────

    private static IEnumerable<Permission> AllPermissions()
    {
        // WorkOrder
        yield return Permission.Create("WorkOrder", "Read");
        yield return Permission.Create("WorkOrder", "Create");
        yield return Permission.Create("WorkOrder", "Update");
        yield return Permission.Create("WorkOrder", "Delete");
        yield return Permission.Create("WorkOrder", "Start");
        yield return Permission.Create("WorkOrder", "Complete");
        yield return Permission.Create("WorkOrder", "Approve");
        // Job
        yield return Permission.Create("Job", "Read");
        yield return Permission.Create("Job", "Start");
        yield return Permission.Create("Job", "Complete");
        // Production
        yield return Permission.Create("Production", "Read");
        yield return Permission.Create("Production", "SubmitOutput");
        // Downtime
        yield return Permission.Create("Downtime", "Declare");
        yield return Permission.Create("Downtime", "Read");
        // Integration
        yield return Permission.Create("Integration", "Read");
        // Inventory
        yield return Permission.Create("Inventory", "Read");
        yield return Permission.Create("Inventory", "Adjust");
        // MasterData
        yield return Permission.Create("MasterData", "Read");
        yield return Permission.Create("MasterData", "Write");
        // User
        yield return Permission.Create("User", "Read");
        yield return Permission.Create("User", "Create");
        yield return Permission.Create("User", "Update");
        yield return Permission.Create("User", "Delete");
        yield return Permission.Create("User", "ManageRoles");
        yield return Permission.Create("User", "ResetPassword");
        // Role
        yield return Permission.Create("Role", "Read");
        yield return Permission.Create("Role", "Create");
        yield return Permission.Create("Role", "Update");
        yield return Permission.Create("Role", "Delete");
        yield return Permission.Create("Role", "ManagePermissions");
        // Report
        yield return Permission.Create("Report", "Read");
        yield return Permission.Create("Report", "Export");
        // Quality
        yield return Permission.Create("Quality", "Read");
        yield return Permission.Create("Quality", "Create");
        yield return Permission.Create("Quality", "Update");
        yield return Permission.Create("Quality", "Approve");
        // NCR
        yield return Permission.Create("NCR", "Read");
        yield return Permission.Create("NCR", "Create");
        yield return Permission.Create("NCR", "Close");
        // Warehouse
        yield return Permission.Create("Warehouse", "Read");
        yield return Permission.Create("Warehouse", "Receive");
        yield return Permission.Create("Warehouse", "PutAway");
        yield return Permission.Create("Warehouse", "Pick");
        yield return Permission.Create("Warehouse", "Dispatch");
        // CycleCount
        yield return Permission.Create("CycleCount", "Read");
        yield return Permission.Create("CycleCount", "Approve");
        // Maintenance
        yield return Permission.Create("Maintenance", "Read");
        yield return Permission.Create("Maintenance", "Execute");
        yield return Permission.Create("Maintenance", "Create");
        yield return Permission.Create("Maintenance", "Approve");
        // System
        yield return Permission.Create("System", "Configure");
        yield return Permission.Create("Permission", "Read");
        yield return Permission.Create("Permission", "Manage");
    }

    // ── Role → Permission matrix ──────────────────────────────────────────

    private static string[] All => [
        "WorkOrder:Read", "WorkOrder:Create", "WorkOrder:Update", "WorkOrder:Delete",
        "WorkOrder:Start", "WorkOrder:Complete", "WorkOrder:Approve",
        "Job:Read", "Job:Start", "Job:Complete",
        "Production:Read", "Production:SubmitOutput",
        "Downtime:Declare", "Downtime:Read",
        "Integration:Read",
        "Inventory:Read", "Inventory:Adjust",
        "MasterData:Read", "MasterData:Write",
        "User:Read", "User:Create", "User:Update", "User:Delete", "User:ManageRoles", "User:ResetPassword",
        "Role:Read", "Role:Create", "Role:Update", "Role:Delete", "Role:ManagePermissions",
        "Report:Read", "Report:Export",
        "Quality:Read", "Quality:Create", "Quality:Update", "Quality:Approve",
        "NCR:Read", "NCR:Create", "NCR:Close",
        "Warehouse:Read", "Warehouse:Receive", "Warehouse:PutAway", "Warehouse:Pick", "Warehouse:Dispatch",
        "CycleCount:Read", "CycleCount:Approve",
        "Maintenance:Read", "Maintenance:Execute", "Maintenance:Create", "Maintenance:Approve",
        "System:Configure", "Permission:Read", "Permission:Manage",
    ];

    private static Dictionary<string, string[]> BuildRolePermissionMatrix() => new()
    {
        [AppRoles.SystemAdmin] = All,

        [AppRoles.FactoryManager] =
        [
            "WorkOrder:Read", "WorkOrder:Approve",
            "Job:Read",
            "Production:Read",
            "Downtime:Read",
            "Inventory:Read",
            "MasterData:Read",
            "User:Read",
            "Role:Read",
            "Report:Read", "Report:Export",
            "Quality:Read", "Quality:Approve",
            "NCR:Read", "NCR:Close",
            "Warehouse:Read",
            "CycleCount:Read", "CycleCount:Approve",
            "Maintenance:Read",
            "Permission:Read",
        ],

        [AppRoles.ProductionSupervisor] =
        [
            "WorkOrder:Read", "WorkOrder:Create", "WorkOrder:Update", "WorkOrder:Start", "WorkOrder:Complete",
            "Job:Read", "Job:Start", "Job:Complete",
            "Production:Read", "Production:SubmitOutput",
            "Downtime:Declare", "Downtime:Read",
            "Inventory:Read",
            "MasterData:Read",
            "Report:Read",
        ],

        [AppRoles.ProductionPlanner] =
        [
            "WorkOrder:Read", "WorkOrder:Create", "WorkOrder:Update",
            "Job:Read",
            "Production:Read",
            "Downtime:Read",
            "Inventory:Read",
            "MasterData:Read",
            "Report:Read",
        ],

        [AppRoles.TeamLead] =
        [
            "WorkOrder:Read",
            "Job:Read", "Job:Start", "Job:Complete",
            "Production:Read", "Production:SubmitOutput",
            "Downtime:Declare", "Downtime:Read",
            "Inventory:Read",
            "MasterData:Read",
            "Report:Read",
        ],

        [AppRoles.Operator] =
        [
            "WorkOrder:Read",
            "Job:Read", "Job:Start", "Job:Complete",
            "Production:SubmitOutput",
            "Downtime:Declare",
            "MasterData:Read",
        ],

        [AppRoles.QcInspector] =
        [
            "WorkOrder:Read",
            "Production:Read",
            "MasterData:Read",
            "Quality:Read", "Quality:Create", "Quality:Update",
            "NCR:Read", "NCR:Create",
            "Report:Read",
        ],

        [AppRoles.QualityManager] =
        [
            "WorkOrder:Read",
            "Production:Read",
            "MasterData:Read",
            "Quality:Read", "Quality:Create", "Quality:Update", "Quality:Approve",
            "NCR:Read", "NCR:Create", "NCR:Close",
            "Report:Read", "Report:Export",
        ],

        [AppRoles.WarehouseStaff] =
        [
            "Inventory:Read", "Inventory:Adjust",
            "MasterData:Read",
            "Warehouse:Read", "Warehouse:Receive", "Warehouse:PutAway", "Warehouse:Pick", "Warehouse:Dispatch",
        ],

        [AppRoles.WarehouseManager] =
        [
            "Inventory:Read", "Inventory:Adjust",
            "MasterData:Read",
            "Warehouse:Read", "Warehouse:Receive", "Warehouse:PutAway", "Warehouse:Pick", "Warehouse:Dispatch",
            "CycleCount:Read", "CycleCount:Approve",
            "Report:Read", "Report:Export",
        ],

        [AppRoles.MaintenanceTech] =
        [
            "MasterData:Read",
            "Maintenance:Read", "Maintenance:Execute",
        ],

        [AppRoles.MaintenanceManager] =
        [
            "MasterData:Read",
            "Maintenance:Read", "Maintenance:Execute", "Maintenance:Create", "Maintenance:Approve",
            "Report:Read",
        ],

        [AppRoles.ReportViewer] =
        [
            "Report:Read",
            "WorkOrder:Read",
            "Production:Read",
            "Inventory:Read",
            "Quality:Read",
            "MasterData:Read",
        ],

        [AppRoles.ErpIntegration] =
        [
            "WorkOrder:Read",
            "Inventory:Read", "Inventory:Adjust",
            "Production:Read",
            "MasterData:Read",
        ],

        [AppRoles.Device] =
        [
            "Job:Start", "Job:Complete",
            "Production:SubmitOutput",
            "Downtime:Declare",
        ],
    };
}
