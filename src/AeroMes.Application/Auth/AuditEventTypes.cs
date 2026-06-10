namespace AeroMes.Application.Auth;

public static class AuditEventTypes
{
    public const string AuthLoginSuccess = "AUTH_LOGIN_SUCCESS";
    public const string AuthLoginFailure = "AUTH_LOGIN_FAILURE";
    public const string AuthLogout = "AUTH_LOGOUT";
    public const string AuthTokenRefresh = "AUTH_TOKEN_REFRESH";
    public const string AuthTokenReuseAttack = "AUTH_TOKEN_REUSE_ATTACK";
    public const string AuthMfaSuccess = "AUTH_MFA_SUCCESS";
    public const string AuthMfaFailure = "AUTH_MFA_FAILURE";
    public const string AuthMfaSetup = "AUTH_MFA_SETUP";
    public const string AuthMfaDisabled = "AUTH_MFA_DISABLED";
    public const string AuthPasskeyRegistered = "AUTH_PASSKEY_REGISTERED";
    public const string AuthPasskeyLogin = "AUTH_PASSKEY_LOGIN";
    public const string AuthPasskeyRemoved = "AUTH_PASSKEY_REMOVED";
    public const string AuthPasswordChanged = "AUTH_PASSWORD_CHANGED";

    public const string UserCreated = "USER_CREATED";
    public const string UserDeactivated = "USER_DEACTIVATED";
    public const string UserActivated = "USER_ACTIVATED";

    public const string RoleAssigned = "ROLE_ASSIGNED";
    public const string RoleRemoved = "ROLE_REMOVED";

    public const string PermissionOverrideGranted = "PERMISSION_OVERRIDE_GRANTED";
    public const string PermissionOverrideRevoked = "PERMISSION_OVERRIDE_REVOKED";
    public const string RolePermissionChanged = "ROLE_PERMISSION_CHANGED";

    public const string ApiKeyCreated = "APIKEY_CREATED";
    public const string ApiKeyRevoked = "APIKEY_REVOKED";
    public const string ApiKeyRotated = "APIKEY_ROTATED";

    public const string PermissionDenied = "PERMISSION_DENIED";

    public const string WorkOrderStarted = "WORK_ORDER_STARTED";
    public const string WorkOrderCompleted = "WORK_ORDER_COMPLETED";
    public const string InventoryAdjusted = "INVENTORY_ADJUSTED";
    public const string CycleCountApproved = "CYCLE_COUNT_APPROVED";
}
