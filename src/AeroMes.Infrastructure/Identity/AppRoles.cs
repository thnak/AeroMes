namespace AeroMes.Infrastructure.Identity;

public static class AppRoles
{
    public const string SystemAdmin = "SYSTEM_ADMIN";
    public const string FactoryManager = "FACTORY_MANAGER";
    public const string ProductionSupervisor = "PRODUCTION_SUPERVISOR";
    public const string ProductionPlanner = "PRODUCTION_PLANNER";
    public const string TeamLead = "TEAM_LEAD";
    public const string Operator = "OPERATOR";
    public const string QcInspector = "QC_INSPECTOR";
    public const string QualityManager = "QUALITY_MANAGER";
    public const string WarehouseStaff = "WAREHOUSE_STAFF";
    public const string WarehouseManager = "WAREHOUSE_MANAGER";
    public const string MaintenanceTech = "MAINTENANCE_TECH";
    public const string MaintenanceManager = "MAINTENANCE_MANAGER";
    public const string ReportViewer = "REPORT_VIEWER";
    public const string ErpIntegration = "ERP_INTEGRATION";
    public const string Device = "DEVICE";

    public static readonly string[] All =
    [
        SystemAdmin, FactoryManager, ProductionSupervisor, ProductionPlanner,
        TeamLead, Operator, QcInspector, QualityManager,
        WarehouseStaff, WarehouseManager, MaintenanceTech, MaintenanceManager,
        ReportViewer, ErpIntegration, Device,
    ];
}
