namespace AeroMes.Api.Auth;

public static class Permissions
{
    // WorkOrder
    public const string WorkOrderRead = "WorkOrder:Read";
    public const string WorkOrderCreate = "WorkOrder:Create";
    public const string WorkOrderUpdate = "WorkOrder:Update";
    public const string WorkOrderDelete = "WorkOrder:Delete";
    public const string WorkOrderStart = "WorkOrder:Start";
    public const string WorkOrderComplete = "WorkOrder:Complete";
    public const string WorkOrderApprove = "WorkOrder:Approve";

    // Job
    public const string JobRead = "Job:Read";
    public const string JobStart = "Job:Start";
    public const string JobComplete = "Job:Complete";

    // Production
    public const string ProductionRead = "Production:Read";
    public const string ProductionSubmitOutput = "Production:SubmitOutput";

    // Downtime
    public const string DowntimeDeclare = "Downtime:Declare";
    public const string DowntimeRead = "Downtime:Read";

    // Integration (ERP)
    public const string IntegrationRead = "Integration:Read";
    public const string IntegrationSync = "Integration:Sync";
    public const string IntegrationConfigure = "Integration:Configure";

    // Inventory
    public const string InventoryRead = "Inventory:Read";
    public const string InventoryAdjust = "Inventory:Adjust";

    // MasterData
    public const string MasterDataRead = "MasterData:Read";
    public const string MasterDataWrite = "MasterData:Write";

    // User
    public const string UserRead = "User:Read";
    public const string UserCreate = "User:Create";
    public const string UserUpdate = "User:Update";
    public const string UserDelete = "User:Delete";
    public const string UserManageRoles = "User:ManageRoles";
    public const string UserResetPassword = "User:ResetPassword";

    // Role
    public const string RoleRead = "Role:Read";
    public const string RoleCreate = "Role:Create";
    public const string RoleUpdate = "Role:Update";
    public const string RoleDelete = "Role:Delete";
    public const string RoleManagePermissions = "Role:ManagePermissions";

    // Report
    public const string ReportRead = "Report:Read";
    public const string ReportExport = "Report:Export";

    // Quality
    public const string QualityRead = "Quality:Read";
    public const string QualityWrite = "Quality:Write";
    public const string QualityCreate = "Quality:Create";
    public const string QualityUpdate = "Quality:Update";
    public const string QualityApprove = "Quality:Approve";

    // NCR
    public const string NcrRead = "NCR:Read";
    public const string NcrCreate = "NCR:Create";
    public const string NcrClose = "NCR:Close";

    // Warehouse
    public const string WarehouseRead = "Warehouse:Read";
    public const string WarehouseReceive = "Warehouse:Receive";
    public const string WarehousePutAway = "Warehouse:PutAway";
    public const string WarehousePick = "Warehouse:Pick";
    public const string WarehouseDispatch = "Warehouse:Dispatch";

    // CycleCount
    public const string CycleCountRead = "CycleCount:Read";
    public const string CycleCountApprove = "CycleCount:Approve";

    // Maintenance
    public const string MaintenanceRead = "Maintenance:Read";
    public const string MaintenanceExecute = "Maintenance:Execute";
    public const string MaintenanceCreate = "Maintenance:Create";
    public const string MaintenanceApprove = "Maintenance:Approve";

    // ApiKey
    public const string ApiKeyRead = "ApiKey:Read";
    public const string ApiKeyCreate = "ApiKey:Create";
    public const string ApiKeyRevoke = "ApiKey:Revoke";

    // IoT
    public const string IotRead = "Iot:Read";
    public const string IotWrite = "Iot:Write";

    // System
    public const string SystemConfigure = "System:Configure";
    public const string PermissionRead = "Permission:Read";
    public const string PermissionManage = "Permission:Manage";

    public static readonly string[] All =
    [
        IntegrationRead, IntegrationSync, IntegrationConfigure,
        WorkOrderRead, WorkOrderCreate, WorkOrderUpdate, WorkOrderDelete, WorkOrderStart, WorkOrderComplete, WorkOrderApprove,
        JobRead, JobStart, JobComplete,
        ProductionRead, ProductionSubmitOutput,
        DowntimeDeclare, DowntimeRead,
        InventoryRead, InventoryAdjust,
        MasterDataRead, MasterDataWrite,
        UserRead, UserCreate, UserUpdate, UserDelete, UserManageRoles, UserResetPassword,
        RoleRead, RoleCreate, RoleUpdate, RoleDelete, RoleManagePermissions,
        ReportRead, ReportExport,
        QualityRead, QualityWrite, QualityCreate, QualityUpdate, QualityApprove,
        NcrRead, NcrCreate, NcrClose,
        WarehouseRead, WarehouseReceive, WarehousePutAway, WarehousePick, WarehouseDispatch,
        CycleCountRead, CycleCountApprove,
        MaintenanceRead, MaintenanceExecute, MaintenanceCreate, MaintenanceApprove,
        ApiKeyRead, ApiKeyCreate, ApiKeyRevoke,
        SystemConfigure, PermissionRead, PermissionManage,
        IotRead, IotWrite,
    ];
}
