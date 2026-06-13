using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Api.Middleware;
using AeroMes.Application.Inventory.Queries.GetInventoryStock;
using AeroMes.Application.Inventory.Queries.GetLotTrace;
using AeroMes.Application.Reports.Queries.GetDowntimeReport;
using AeroMes.Application.Reports.Queries.GetProductionReport;
using AeroMes.Application.Reports.Queries.GetQualityReport;
using AeroMes.Application.Integration.Commands.SaveErpSettings;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using AeroMes.Application.Integration.Queries.GetErpSettings;
using AeroMes.Application.Integration.Queries.GetProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetProductionOrders;
using AeroMes.Application.Integration.Queries.GetSalesOrderDetail;
using AeroMes.Application.Integration.Queries.GetSalesOrders;
using AeroMes.Application.Common;
using AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using AeroMes.Api.Services;
using AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Application.Jobs.Queries.GetJobDetail;
using AeroMes.Application.Jobs.Queries.GetJobs;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;
using AeroMes.Application.Master.CapabilityGroupMembers.Queries.GetMembers;
using AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;
using AeroMes.Application.Master.DowntimeReasonCodes.Queries.GetDowntimeReasonCodes;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategoryTree;
using AeroMes.Application.Master.Products.Queries.GetProducts;
using AeroMes.Application.Master.Products.Queries.GetProductSpecifications;
using AeroMes.Application.Master.Products.Queries.GetProductVariants;
using AeroMes.Application.Master.ShiftTemplates.Queries.GetShiftTemplates;
using AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;
using AeroMes.Application.Master.Customers.Queries.GetCustomerById;
using AeroMes.Application.Master.Customers.Queries.GetCustomers;
using AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using AeroMes.Application.Master.Employees.Queries.GetEmployees;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;
using AeroMes.Application.Master.Boms.Queries.CompareBomVersions;
using AeroMes.Application.Master.Boms.Queries.ExplodeBom;
using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using AeroMes.Application.Master.Boms.Queries.GetBomVersions;
using AeroMes.Application.Master.EngChanges.Commands.ImplementEco;
using AeroMes.Application.Master.EngChanges.Queries.GetEngChangeByNumber;
using AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;
using AeroMes.Application.Master.Molds.Commands.RecordMoldShots;
using AeroMes.Application.Master.Molds.Queries.GetMoldByCode;
using AeroMes.Application.Master.Molds.Queries.GetMolds;
using AeroMes.Application.Master.Molds.Queries.GetMoldsDueForPm;
using AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitById;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitTree;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeamByCode;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeams;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;
using AeroMes.Application.Master.Suppliers.Queries.GetSupplierById;
using AeroMes.Application.Master.Tools.Commands.RecordToolUsage;
using AeroMes.Application.Master.Tools.Queries.GetToolByCode;
using AeroMes.Application.Master.Tools.Queries.GetTools;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForCalibration;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForReconditioning;
using AeroMes.Application.Master.Suppliers.Queries.GetSuppliers;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;
using AeroMes.Application.Master.Machines.Queries.GetMachines;
using AeroMes.Application.Master.MachineProductConfigs.Queries.GetMachineProductConfigs;
using AeroMes.Application.Master.MachineProductParams.Queries.GetMachineSetupSheet;
using AeroMes.Application.Master.OperatorCertifications.Queries.CheckOperatorEligibility;
using AeroMes.Application.Master.OperatorCertifications.Queries.GetOperatorCertifications;
using AeroMes.Application.Master.Warehouses.Queries.GetWarehouses;
using AeroMes.Application.Wms.Queries.GetAisles;
using AeroMes.Application.Wms.Queries.GetBins;
using AeroMes.Application.Wms.Queries.GetBinStock;
using AeroMes.Application.Wms.Queries.GetRacks;
using AeroMes.Application.Wms.Queries.GetWarehouseMap;
using AeroMes.Application.Wms.Queries.GetZones;
using AeroMes.Application.Wms.Commands.CreatePurchaseOrder;
using AeroMes.Application.Wms.Commands.CreateGrn;
using AeroMes.Application.Wms.Commands.AddGrnLine;
using AeroMes.Application.Wms.Queries.GetPurchaseOrders;
using AeroMes.Application.Wms.Queries.GetGrnList;
using AeroMes.Application.Wms.Queries.GetGrnDetail;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Constants;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(AeroMes.Domain.Settings.SystemOptions))]
[JsonSerializable(typeof(SimpleProblemResponse))]
[JsonSerializable(typeof(ValidationProblemResponse))]
// auth
[JsonSerializable(typeof(ForgotPasswordRequest))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(MfaPendingResult))]
[JsonSerializable(typeof(RefreshTokenResult))]
[JsonSerializable(typeof(UserProfileResult))]
[JsonSerializable(typeof(UpdateMeRequest))]
[JsonSerializable(typeof(ChangePasswordRequest))]
[JsonSerializable(typeof(MfaEnforcementMiddleware.MfaVerifyRequiredResponse))]
[JsonSerializable(typeof(ForcePasswordChangeMiddleware.ForcePasswordChangeResponse))]
// machines
[JsonSerializable(typeof(IReadOnlyList<MachineDto>))]
[JsonSerializable(typeof(MachineCreatedResult))]
[JsonSerializable(typeof(CreateMachineRequest))]
[JsonSerializable(typeof(UpdateMachineRequest))]
[JsonSerializable(typeof(UpdateMachineCapacityRequest))]
// machine product configs
[JsonSerializable(typeof(IReadOnlyList<MachineProductConfigDto>))]
[JsonSerializable(typeof(UpsertMachineProductConfigRequest))]
// machine product params
[JsonSerializable(typeof(IReadOnlyList<MachineProductParamDto>))]
[JsonSerializable(typeof(UpsertMachineProductParamRequest))]
// operator certifications
[JsonSerializable(typeof(IReadOnlyList<OperatorCertificationDto>))]
[JsonSerializable(typeof(OperatorEligibilityResult))]
[JsonSerializable(typeof(RecordOperatorCertificationRequest))]
[JsonSerializable(typeof(CertificationIssuedResult))]
// capability groups
[JsonSerializable(typeof(IReadOnlyList<CapabilityGroupDto>))]
[JsonSerializable(typeof(CapabilityGroupCreatedResult))]
[JsonSerializable(typeof(IReadOnlyList<CapabilityGroupMemberDto>))]
[JsonSerializable(typeof(AssignMemberRequest))]
[JsonSerializable(typeof(MemberAssignedResult))]
// work shifts
[JsonSerializable(typeof(IReadOnlyList<WorkShiftDto>))]
[JsonSerializable(typeof(WorkShiftDetailDto))]
[JsonSerializable(typeof(WorkShiftCreatedResult))]
// work calendars
[JsonSerializable(typeof(IReadOnlyList<WorkCalendarDto>))]
[JsonSerializable(typeof(WorkCalendarDetailDto))]
[JsonSerializable(typeof(WorkCalendarCreatedResult))]
[JsonSerializable(typeof(CalendarExceptionCreatedResult))]
[JsonSerializable(typeof(AddCalendarExceptionRequest))]
[JsonSerializable(typeof(CreateWorkCalendarRequest))]
[JsonSerializable(typeof(UpdateWorkCalendarRequest))]
// shift templates
[JsonSerializable(typeof(IReadOnlyList<ShiftTemplateDto>))]
[JsonSerializable(typeof(ShiftTemplateCreatedResult))]
[JsonSerializable(typeof(CreateShiftTemplateRequest))]
[JsonSerializable(typeof(UpdateShiftTemplateRequest))]
// storage locations
[JsonSerializable(typeof(IReadOnlyList<StorageLocationDto>))]
[JsonSerializable(typeof(StorageLocationCreatedResult))]
[JsonSerializable(typeof(CreateStorageLocationRequest))]
[JsonSerializable(typeof(UpdateStorageLocationRequest))]
// product categories
[JsonSerializable(typeof(IReadOnlyList<ProductCategoryDto>))]
[JsonSerializable(typeof(IReadOnlyList<ProductCategoryTreeDto>))]
[JsonSerializable(typeof(ProductCategoryCreatedResult))]
[JsonSerializable(typeof(CreateProductCategoryRequest))]
[JsonSerializable(typeof(UpdateProductCategoryRequest))]
// products
[JsonSerializable(typeof(IReadOnlyList<ProductDto>))]
[JsonSerializable(typeof(ProductDetailDto))]
[JsonSerializable(typeof(ProductCreatedResult))]
[JsonSerializable(typeof(CreateProductRequest))]
[JsonSerializable(typeof(UpdateProductRequest))]
[JsonSerializable(typeof(ChangeLifecycleRequest))]
[JsonSerializable(typeof(IReadOnlyList<ProductUoMConversionDto>))]
[JsonSerializable(typeof(AddUoMConversionRequest))]
[JsonSerializable(typeof(UpdateUoMConversionRequest))]
[JsonSerializable(typeof(UoMConversionCreatedResult))]
// variants / specification codes
[JsonSerializable(typeof(IReadOnlyList<ProductVariantDto>))]
[JsonSerializable(typeof(IReadOnlyList<ProductSpecificationDto>))]
[JsonSerializable(typeof(CreateVariantRequest))]
[JsonSerializable(typeof(AddSpecificationRequest))]
[JsonSerializable(typeof(UpdateSpecificationRequest))]
[JsonSerializable(typeof(VariantCreatedResult))]
[JsonSerializable(typeof(SpecificationCreatedResult))]
// downtime reason codes
[JsonSerializable(typeof(IReadOnlyList<DowntimeReasonCodeDto>))]
[JsonSerializable(typeof(DowntimeReasonCodeCreatedResult))]
[JsonSerializable(typeof(CreateDowntimeReasonCodeRequest))]
[JsonSerializable(typeof(UpdateDowntimeReasonCodeRequest))]
// module status
[JsonSerializable(typeof(ApiResponse<ModuleStatusResponse>))]
[JsonSerializable(typeof(ApiResponse<ModuleStatusDto>))]
[JsonSerializable(typeof(ModuleStatusResponse))]
[JsonSerializable(typeof(ModuleStatusDto))]
[JsonSerializable(typeof(IReadOnlyList<ModuleStatusDto>))]
[JsonSerializable(typeof(BadgeDto))]
[JsonSerializable(typeof(IReadOnlyList<BadgeDto>))]
[JsonSerializable(typeof(AlertItemDto))]
[JsonSerializable(typeof(IReadOnlyList<AlertItemDto>))]
[JsonSerializable(typeof(ModuleStatusUpdatedPayload))]
// alert thresholds
[JsonSerializable(typeof(IReadOnlyList<AlertThresholdDto>))]
[JsonSerializable(typeof(AlertThresholdCreatedResult))]
[JsonSerializable(typeof(CreateAlertThresholdRequest))]
[JsonSerializable(typeof(UpdateAlertThresholdRequest))]
// inventory
[JsonSerializable(typeof(IReadOnlyList<InventoryStockDto>))]
[JsonSerializable(typeof(LotTraceDto))]
[JsonSerializable(typeof(IReadOnlyList<LotStockEntryDto>))]
// reports
[JsonSerializable(typeof(ProductionReportDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionReportRowDto>))]
[JsonSerializable(typeof(DowntimeReportDto))]
[JsonSerializable(typeof(IReadOnlyList<DowntimeReportRowDto>))]
[JsonSerializable(typeof(QualityReportDto))]
[JsonSerializable(typeof(IReadOnlyList<QualityReportRowDto>))]
// integration
[JsonSerializable(typeof(IReadOnlyList<SalesOrderDto>))]
[JsonSerializable(typeof(SalesOrderDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionOrderSummaryDto>))]
[JsonSerializable(typeof(IReadOnlyList<ProductionOrderDto>))]
[JsonSerializable(typeof(ProductionOrderDetailDto))]
[JsonSerializable(typeof(ApiResponse<ErpSettingsDto>))]
[JsonSerializable(typeof(ErpSettingsDto))]
[JsonSerializable(typeof(SaveErpSettingsRequest))]
[JsonSerializable(typeof(ApiResponse<ErpSyncResult>))]
[JsonSerializable(typeof(ErpSyncResult))]
[JsonSerializable(typeof(ApiResponse<bool>))]
[JsonSerializable(typeof(IReadOnlyList<WorkOrderSummaryDto>))]
// work order detail
[JsonSerializable(typeof(WorkOrderDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<JobSummaryDto>))]
// jobs
[JsonSerializable(typeof(IReadOnlyList<JobDto>))]
[JsonSerializable(typeof(JobDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionLogDto>))]
// downtime
[JsonSerializable(typeof(IReadOnlyList<DowntimeLogDto>))]
[JsonSerializable(typeof(DowntimeLogDto))]
// defect codes
[JsonSerializable(typeof(IReadOnlyList<DefectCodeDto>))]
[JsonSerializable(typeof(DefectCodeCreatedResult))]
[JsonSerializable(typeof(CreateDefectCodeRequest))]
[JsonSerializable(typeof(UpdateDefectCodeRequest))]
// suppliers / AVL
[JsonSerializable(typeof(IReadOnlyList<SupplierDto>))]
[JsonSerializable(typeof(SupplierDetailDto))]
[JsonSerializable(typeof(SupplierCreatedResult))]
[JsonSerializable(typeof(AvlItemCreatedResult))]
[JsonSerializable(typeof(AddAvlItemRequest))]
[JsonSerializable(typeof(UpdateAvlItemRequest))]
// customers
[JsonSerializable(typeof(IReadOnlyList<CustomerDto>))]
[JsonSerializable(typeof(CustomerDetailDto))]
[JsonSerializable(typeof(CustomerPartLookupDto))]
[JsonSerializable(typeof(CustomerCreatedResult))]
[JsonSerializable(typeof(CustomerPartNumberCreatedResult))]
[JsonSerializable(typeof(CustomerQualitySpecSavedResult))]
[JsonSerializable(typeof(CreateCustomerRequest))]
[JsonSerializable(typeof(UpdateCustomerRequest))]
[JsonSerializable(typeof(AddCustomerPartNumberRequest))]
[JsonSerializable(typeof(UpdateCustomerPartNumberRequest))]
[JsonSerializable(typeof(SetCustomerQualitySpecRequest))]
// employees
[JsonSerializable(typeof(IReadOnlyList<EmployeeDto>))]
[JsonSerializable(typeof(EmployeeDetailDto))]
[JsonSerializable(typeof(EmployeeScheduleDto))]
[JsonSerializable(typeof(EmployeeCreatedResult))]
[JsonSerializable(typeof(EmployeeSkillSavedResult))]
[JsonSerializable(typeof(ShiftAssignmentCreatedResult))]
[JsonSerializable(typeof(CreateEmployeeRequest))]
[JsonSerializable(typeof(UpdateEmployeeRequest))]
[JsonSerializable(typeof(SetEmployeeSkillRequest))]
[JsonSerializable(typeof(AddShiftAssignmentRequest))]
[JsonSerializable(typeof(EndShiftAssignmentRequest))]
// org units
[JsonSerializable(typeof(IReadOnlyList<OrgUnitDto>))]
[JsonSerializable(typeof(IReadOnlyList<OrgUnitTreeDto>))]
[JsonSerializable(typeof(OrgUnitDetailDto))]
[JsonSerializable(typeof(SyncOrgUnitsRequest))]
[JsonSerializable(typeof(SyncOrgUnitsResult))]
// production teams
[JsonSerializable(typeof(IReadOnlyList<ProductionTeamDto>))]
[JsonSerializable(typeof(ProductionTeamDetailDto))]
[JsonSerializable(typeof(ProductionTeamCreatedResult))]
[JsonSerializable(typeof(TeamMemberAddedResult))]
[JsonSerializable(typeof(CreateProductionTeamRequest))]
[JsonSerializable(typeof(UpdateProductionTeamRequest))]
[JsonSerializable(typeof(DuplicateTeamRequest))]
[JsonSerializable(typeof(AddTeamMemberRequest))]
// product attributes
[JsonSerializable(typeof(IReadOnlyList<ProductAttributeDto>))]
[JsonSerializable(typeof(ProductAttributeDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductAttributeAssignmentDto>))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
[JsonSerializable(typeof(ProductAttributeCreatedResult))]
[JsonSerializable(typeof(AttributeValueCreatedResult))]
[JsonSerializable(typeof(AttributeAssignedResult))]
[JsonSerializable(typeof(CreateProductAttributeRequest))]
[JsonSerializable(typeof(UpdateProductAttributeRequest))]
[JsonSerializable(typeof(AttributeValueRequest))]
[JsonSerializable(typeof(AssignAttributeRequest))]
// molds
[JsonSerializable(typeof(IReadOnlyList<MoldDto>))]
[JsonSerializable(typeof(MoldDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<MoldPmDueDto>))]
[JsonSerializable(typeof(MoldCreatedResult))]
[JsonSerializable(typeof(MoldProductAddedResult))]
[JsonSerializable(typeof(MoldMaintenanceLoggedResult))]
[JsonSerializable(typeof(RecordMoldShotsResult))]
[JsonSerializable(typeof(RegisterMoldRequest))]
[JsonSerializable(typeof(UpdateMoldRequest))]
[JsonSerializable(typeof(AddMoldProductRequest))]
[JsonSerializable(typeof(AssignMoldRequest))]
[JsonSerializable(typeof(SendMoldMaintenanceRequest))]
[JsonSerializable(typeof(CompleteMoldMaintenanceRequest))]
[JsonSerializable(typeof(RecordMoldShotsRequest))]
// tools
[JsonSerializable(typeof(IReadOnlyList<ToolDto>))]
[JsonSerializable(typeof(ToolDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ToolCalibrationDueDto>))]
[JsonSerializable(typeof(IReadOnlyList<ToolReconditioningDueDto>))]
[JsonSerializable(typeof(ToolCreatedResult))]
[JsonSerializable(typeof(ToolOperationAddedResult))]
[JsonSerializable(typeof(ToolCheckedOutResult))]
[JsonSerializable(typeof(ToolMaintenanceLoggedResult))]
[JsonSerializable(typeof(RecordToolUsageResult))]
[JsonSerializable(typeof(RegisterToolRequest))]
[JsonSerializable(typeof(UpdateToolRequest))]
[JsonSerializable(typeof(AddToolOperationRequest))]
[JsonSerializable(typeof(CheckoutToolRequest))]
[JsonSerializable(typeof(ReturnToolRequest))]
[JsonSerializable(typeof(SendToolServiceRequest))]
[JsonSerializable(typeof(RecordToolMaintenanceRequest))]
[JsonSerializable(typeof(RecordToolUsageRequest))]
// health
[JsonSerializable(typeof(HealthStatus))]
// app info
[JsonSerializable(typeof(ApiResponse<AppInfoDto>))]
[JsonSerializable(typeof(AppInfoDto))]
// versioned BOM + engineering changes
[JsonSerializable(typeof(BomVersionDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<BomVersionDto>))]
[JsonSerializable(typeof(IReadOnlyList<ExplodedBomLineDto>))]
[JsonSerializable(typeof(BomCompareDto))]
[JsonSerializable(typeof(BomDraftCreatedResult))]
[JsonSerializable(typeof(CreateBomDraftRequest))]
[JsonSerializable(typeof(UpdateBomLinesRequest))]
[JsonSerializable(typeof(ActivateBomRequest))]
[JsonSerializable(typeof(IReadOnlyList<EngChangeDto>))]
[JsonSerializable(typeof(EngChangeDetailDto))]
[JsonSerializable(typeof(EngChangeCreatedResult))]
[JsonSerializable(typeof(ImplementEcoResult))]
[JsonSerializable(typeof(CreateEngChangeRequest))]
[JsonSerializable(typeof(CreateEcoRequest))]
[JsonSerializable(typeof(ImplementEcoRequest))]
// warehouse catalog
[JsonSerializable(typeof(IReadOnlyList<WarehouseDto>))]
[JsonSerializable(typeof(WarehouseCreatedResult))]
[JsonSerializable(typeof(CreateWarehouseRequest))]
[JsonSerializable(typeof(UpdateWarehouseRequest))]
// machine additions
[JsonSerializable(typeof(DuplicateMachineRequest))]
// purchase orders & grn
[JsonSerializable(typeof(IReadOnlyList<PurchaseOrderDto>))]
[JsonSerializable(typeof(PoCreatedResult))]
[JsonSerializable(typeof(CreatePurchaseOrderRequest))]
[JsonSerializable(typeof(IReadOnlyList<GrnListDto>))]
[JsonSerializable(typeof(GrnDetailDto))]
[JsonSerializable(typeof(GrnCreatedResult))]
[JsonSerializable(typeof(CreateGrnRequest))]
[JsonSerializable(typeof(GrnLineAddedResult))]
[JsonSerializable(typeof(AddGrnLineRequest))]
// wms location hierarchy
[JsonSerializable(typeof(IReadOnlyList<ZoneDto>))]
[JsonSerializable(typeof(ZoneCreatedResult))]
[JsonSerializable(typeof(CreateZoneRequest))]
[JsonSerializable(typeof(UpdateZoneRequest))]
[JsonSerializable(typeof(IReadOnlyList<AisleDto>))]
[JsonSerializable(typeof(AisleCreatedResult))]
[JsonSerializable(typeof(CreateAisleRequest))]
[JsonSerializable(typeof(IReadOnlyList<RackDto>))]
[JsonSerializable(typeof(RackCreatedResult))]
[JsonSerializable(typeof(CreateRackRequest))]
[JsonSerializable(typeof(IReadOnlyList<BinDto>))]
[JsonSerializable(typeof(BinCreatedResult))]
[JsonSerializable(typeof(CreateBinRequest))]
[JsonSerializable(typeof(UpdateBinRequest))]
[JsonSerializable(typeof(IReadOnlyList<BinStockDto>))]
[JsonSerializable(typeof(IReadOnlyList<ZoneMapDto>))]
[JsonSerializable(typeof(ZoneMapDto))]
public partial class ApiJsonContext : JsonSerializerContext
{

}
