using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AeroMes.IntegrationTests.Auth;

public class PermissionHandlerTests
{
    private const string UserId = "test-user-id";
    private const string PermCode = "WorkOrder:Start";

    // ── Stubs ────────────────────────────────────────────────────────────────

    private sealed class StubPermissionService(bool grant) : IPermissionService
    {
        public Task<bool> HasPermissionAsync(string userId, string permissionCode, CancellationToken ct = default)
            => Task.FromResult(grant);
        public Task InvalidateCacheAsync(string userId) => Task.CompletedTask;
    }

    private sealed class CapturingAuditLogger : IAuditLogger
    {
        public List<SecurityAuditEvent> Events { get; } = [];
        public void Log(SecurityAuditEvent e) => Events.Add(e);
    }

    private static PermissionAuthorizationHandler BuildHandler(
        bool permissionGranted, out CapturingAuditLogger logger)
    {
        logger = new CapturingAuditLogger();
        return new PermissionAuthorizationHandler(
            new StubPermissionService(permissionGranted),
            logger,
            new HttpContextAccessor());   // HttpContext = null — handler handles it gracefully
    }

    private static AuthorizationHandlerContext ContextFor(ClaimsPrincipal user)
    {
        var requirement = new PermissionRequirement(PermCode);
        return new AuthorizationHandlerContext([requirement], user, resource: null);
    }

    private static ClaimsPrincipal AuthenticatedUser(string? userId = UserId)
    {
        var claims = userId is null
            ? Array.Empty<Claim>()
            : [new Claim(ClaimTypes.NameIdentifier, userId)];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    // ── Tests ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NoUserIdClaim_Fails()
    {
        var handler = BuildHandler(permissionGranted: true, out _);
        var context = ContextFor(AuthenticatedUser(userId: null));

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task UserHasPermission_Succeeds()
    {
        var handler = BuildHandler(permissionGranted: true, out var logger);
        var context = ContextFor(AuthenticatedUser());

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Empty(logger.Events);
    }

    [Fact]
    public async Task UserLacksPermission_FailsAndLogsAuditEvent()
    {
        var handler = BuildHandler(permissionGranted: false, out var logger);
        var context = ContextFor(AuthenticatedUser());

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.Single(logger.Events);
        var evt = logger.Events[0];
        Assert.Equal(AuditEventTypes.PermissionDenied, evt.EventType);
        Assert.Equal(UserId, evt.ActorId);
        Assert.Equal(PermCode, evt.TargetId);
        Assert.Equal("FAILURE", evt.Outcome);
    }

    [Fact]
    public async Task DenyOverrideBeatsRoleGrant_Fails()
    {
        // PermissionService encapsulates DENY > GRANT logic;
        // when it returns false the handler must always fail regardless of context.
        var handler = BuildHandler(permissionGranted: false, out _);
        var context = ContextFor(AuthenticatedUser());

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
