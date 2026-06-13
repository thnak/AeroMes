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
    public const string BeginningInventoryWrite = "BeginningInventory:Write";

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

    // Factory Warehouse Receipt
    public const string FactoryWarehouseReceiptRead = "FactoryWarehouseReceipt:Read";
    public const string FactoryWarehouseReceiptCreate = "FactoryWarehouseReceipt:Create";
    public const string FactoryWarehouseReceiptUpdate = "FactoryWarehouseReceipt:Update";
    public const string FactoryWarehouseReceiptDelete = "FactoryWarehouseReceipt:Delete";

    // Factory Warehouse Export
    public const string FactoryWarehouseExportRead = "FactoryWarehouseExport:Read";
    public const string FactoryWarehouseExportCreate = "FactoryWarehouseExport:Create";
    public const string FactoryWarehouseExportUpdate = "FactoryWarehouseExport:Update";
    public const string FactoryWarehouseExportDelete = "FactoryWarehouseExport:Delete";

    // Material Transfer Slip
    public const string MaterialTransferRead = "MaterialTransfer:Read";
    public const string MaterialTransferCreate = "MaterialTransfer:Create";
    public const string MaterialTransferUpdate = "MaterialTransfer:Update";
    public const string MaterialTransferDelete = "MaterialTransfer:Delete";

    // Finished Product Intake Request
    public const string FinishedProductIntakeRead = "FinishedProductIntake:Read";
    public const string FinishedProductIntakeCreate = "FinishedProductIntake:Create";
    public const string FinishedProductIntakeUpdate = "FinishedProductIntake:Update";
    public const string FinishedProductIntakeDelete = "FinishedProductIntake:Delete";
    public const string FinishedProductIntakeSend = "FinishedProductIntake:Send";
    public const string FinishedProductIntakeRecall = "FinishedProductIntake:Recall";
    public const string FinishedProductIntakeReceive = "FinishedProductIntake:Receive";

    // Material Requisition
    public const string MaterialRequisitionRead = "MaterialRequisition:Read";
    public const string MaterialRequisitionCreate = "MaterialRequisition:Create";
    public const string MaterialRequisitionUpdate = "MaterialRequisition:Update";
    public const string MaterialRequisitionDelete = "MaterialRequisition:Delete";
    public const string MaterialRequisitionSend = "MaterialRequisition:Send";
    public const string MaterialRequisitionRecall = "MaterialRequisition:Recall";
    public const string MaterialRequisitionFulfill = "MaterialRequisition:Fulfill";

    // Material Supply Request
    public const string MaterialSupplyRequestRead = "MaterialSupplyRequest:Read";
    public const string MaterialSupplyRequestCreate = "MaterialSupplyRequest:Create";
    public const string MaterialSupplyRequestUpdate = "MaterialSupplyRequest:Update";
    public const string MaterialSupplyRequestDelete = "MaterialSupplyRequest:Delete";
    public const string MaterialSupplyRequestSubmit = "MaterialSupplyRequest:Submit";
    public const string MaterialSupplyRequestApprove = "MaterialSupplyRequest:Approve";

    // Warehouse
    public const string WarehouseRead = "Warehouse:Read";
    public const string WarehouseReceive = "Warehouse:Receive";
    public const string WarehousePutAway = "Warehouse:PutAway";
    public const string WarehousePick = "Warehouse:Pick";
    public const string WarehouseDispatch = "Warehouse:Dispatch";

    // Stock Policy & Replenishment
    public const string StockPolicyRead = "StockPolicy:Read";
    public const string StockPolicyWrite = "StockPolicy:Write";
    public const string ReplenishmentAlertRead = "ReplenishmentAlert:Read";
    public const string ReplenishmentAlertAcknowledge = "ReplenishmentAlert:Acknowledge";

    // Lot Allocation
    public const string AllocationPreview = "Allocation:Preview";

    // RMA (Returns)
    public const string RmaRead = "Rma:Read";
    public const string RmaCreate = "Rma:Create";
    public const string RmaAuthorize = "Rma:Authorize";
    public const string RmaReceive = "Rma:Receive";
    public const string RmaDispose = "Rma:Dispose";

    // CycleCount
    public const string CycleCountRead = "CycleCount:Read";
    public const string CycleCountCreate = "CycleCount:Create";
    public const string CycleCountExecute = "CycleCount:Execute";
    public const string CycleCountDelete = "CycleCount:Delete";
    public const string CycleCountApprove = "CycleCount:Approve";

    // Shipment (Outbound)
    public const string ShipmentRead = "Shipment:Read";
    public const string ShipmentCreate = "Shipment:Create";
    public const string ShipmentPick = "Shipment:Pick";
    public const string ShipmentPack = "Shipment:Pack";
    public const string ShipmentDispatch = "Shipment:Dispatch";

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

    // File storage
    public const string FileUpload = "File:Upload";
    public const string FileDelete = "File:Delete";

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
        InventoryRead, InventoryAdjust, BeginningInventoryWrite,
        FactoryWarehouseReceiptRead, FactoryWarehouseReceiptCreate, FactoryWarehouseReceiptUpdate, FactoryWarehouseReceiptDelete,
        FactoryWarehouseExportRead, FactoryWarehouseExportCreate, FactoryWarehouseExportUpdate, FactoryWarehouseExportDelete,
        MaterialTransferRead, MaterialTransferCreate, MaterialTransferUpdate, MaterialTransferDelete,
        FinishedProductIntakeRead, FinishedProductIntakeCreate, FinishedProductIntakeUpdate, FinishedProductIntakeDelete, FinishedProductIntakeSend, FinishedProductIntakeRecall, FinishedProductIntakeReceive,
        MaterialRequisitionRead, MaterialRequisitionCreate, MaterialRequisitionUpdate, MaterialRequisitionDelete, MaterialRequisitionSend, MaterialRequisitionRecall, MaterialRequisitionFulfill,
        MaterialSupplyRequestRead, MaterialSupplyRequestCreate, MaterialSupplyRequestUpdate, MaterialSupplyRequestDelete, MaterialSupplyRequestSubmit, MaterialSupplyRequestApprove,
        MasterDataRead, MasterDataWrite,
        UserRead, UserCreate, UserUpdate, UserDelete, UserManageRoles, UserResetPassword,
        RoleRead, RoleCreate, RoleUpdate, RoleDelete, RoleManagePermissions,
        ReportRead, ReportExport,
        QualityRead, QualityWrite, QualityCreate, QualityUpdate, QualityApprove,
        NcrRead, NcrCreate, NcrClose,
        WarehouseRead, WarehouseReceive, WarehousePutAway, WarehousePick, WarehouseDispatch,
        StockPolicyRead, StockPolicyWrite,
        ReplenishmentAlertRead, ReplenishmentAlertAcknowledge,
        CycleCountRead, CycleCountCreate, CycleCountExecute, CycleCountDelete, CycleCountApprove,
        AllocationPreview,
        RmaRead, RmaCreate, RmaAuthorize, RmaReceive, RmaDispose,
        ShipmentRead, ShipmentCreate, ShipmentPick, ShipmentPack, ShipmentDispatch,
        MaintenanceRead, MaintenanceExecute, MaintenanceCreate, MaintenanceApprove,
        ApiKeyRead, ApiKeyCreate, ApiKeyRevoke,
        SystemConfigure, PermissionRead, PermissionManage,
        IotRead, IotWrite,
        FileUpload, FileDelete,
    ];
}
