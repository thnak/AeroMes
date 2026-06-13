using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Api.Middleware;
using AeroMes.Application.Inventory.Queries.GetInventoryStock;
using AeroMes.Application.Inventory.Queries.GetLotTrace;
using AeroMes.Application.Production.Queries.GetAvailableStock;
using AeroMes.Application.Production.Queries.GetInventoryByExpiry;
using AeroMes.Application.Storage.Commands.UploadFile;
using AeroMes.Application.Storage.Queries.GetFileMetadata;
using AeroMes.Application.Reports.Queries.GetDowntimeReport;
using AeroMes.Application.Reports.Queries.GetProductionReport;
using AeroMes.Application.Reports.Queries.GetQualityReport;
using AeroMes.Application.Integration.Commands.BatchCreateProductionOrders;
using AeroMes.Application.Integration.Commands.CreateMultiProductionOrder;
using AeroMes.Application.Integration.Commands.SaveErpSettings;
using AeroMes.Application.Integration.Queries.GetMultiProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetMultiProductionOrders;
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
using AeroMes.Application.Master.Machines.Queries.GetMachinesByType;
using AeroMes.Application.Master.Machines.Queries.GetCompatibleMachinesForMold;
using AeroMes.Application.Master.ProductFamilies.Queries.GetProductFamilies;
using AeroMes.Application.Master.ProductFamilies.Queries.GetVariantMatrix;
using AeroMes.Application.Master.ProductFamilies.Queries.GetVariantByAttributes;
using AeroMes.Application.Production.StageHandovers.Queries.GetHandoverForms;
using AeroMes.Application.Production.StageHandovers.Queries.GetHandoverFormDetail;
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
using AeroMes.Application.Wms.Commands.CreateBeginningInventoryEntry;
using AeroMes.Application.Wms.Commands.UpdateBeginningInventoryEntry;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;
using AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseReceipt;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceipts;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceiptById;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseExports;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseExportById;
using AeroMes.Application.Wms.Commands.CreateMaterialTransferSlip;
using AeroMes.Application.Wms.Queries.GetMaterialTransferSlips;
using AeroMes.Application.Wms.Queries.GetMaterialTransferSlipById;
using AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;
using AeroMes.Application.Wms.Queries.GetMaterialSupplyRequests;
using AeroMes.Application.Wms.Queries.GetMaterialSupplyRequestById;
using AeroMes.Application.Wms.Commands.CreateMaterialRequisition;
using AeroMes.Application.Wms.Commands.FulfillMaterialRequisition;
using AeroMes.Application.Wms.Queries.GetMaterialRequisitions;
using AeroMes.Application.Wms.Queries.GetMaterialRequisitionById;
using AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.ReceiveFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequests;
using AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequestById;
using AeroMes.Application.Wms.Commands.CreateCycleCountPlan;
using AeroMes.Application.Wms.Commands.ApproveCycleCount;
using AeroMes.Application.Wms.Queries.GetCycleCountPlans;
using AeroMes.Application.Wms.Queries.GetCycleCountPlanById;
using AeroMes.Application.Wms.Queries.GetCycleCountSheet;
using AeroMes.Application.Wms.Commands.CreateStockPolicy;
using AeroMes.Application.Wms.Commands.UpdateStockPolicy;
using AeroMes.Application.Wms.Queries.GetStockPolicies;
using AeroMes.Application.Wms.Queries.GetReplenishmentAlerts;
using AeroMes.Application.Wms.Queries.GetStockStatus;
using AeroMes.Application.Wms.Queries.GetPurchaseOrders;
using AeroMes.Application.Wms.Queries.GetGrnList;
using AeroMes.Application.Wms.Queries.GetGrnDetail;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapters;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;
using AeroMes.Application.Iot.Signals;
using AeroMes.Application.Iot.Signals.Queries.GetSignals;
using AeroMes.Application.Iot.StateRules.Queries.GetStateRules;
using AeroMes.Infrastructure.Iot;
using AeroMes.Application.Quality.InspectionOrders;
using AeroMes.Application.Quality.InspectionPlans;
using AeroMes.Application.Quality.InspectionResults;
using AeroMes.Application.Quality.Ncr;
using AeroMes.Application.Wms.Queries.GetBeginningInventoryEntries;
using AeroMes.Application.Wms.Commands.UpdateProductPickingConfig;
using AeroMes.Application.Wms.Queries.GetLotAllocation;
using AeroMes.Application.Wms.Services;
using AeroMes.Application.Wms.Commands.CreateRma;
using AeroMes.Application.Wms.Commands.AddRmaLine;
using AeroMes.Application.Wms.Commands.ReceiveRma;
using AeroMes.Application.Wms.Queries.GetRmaList;
using AeroMes.Application.Wms.Queries.GetRmaById;
using AeroMes.Application.Wms.Commands.CreateShipmentOrder;
using AeroMes.Application.Wms.Commands.CreatePickList;
using AeroMes.Application.Wms.Commands.CreateCarton;
using AeroMes.Application.Wms.Queries.GetShipmentList;
using AeroMes.Application.Wms.Queries.GetShipmentById;
using AeroMes.Application.Wms.Queries.GetPickListForShipment;
using AeroMes.Application.Master.SubstituteMaterials.Commands.CreateSubstituteMaterial;
using AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;
using AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;
using AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBoms;
using AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBomById;
using AeroMes.Application.Master.Boms.Commands.UpdateBomByProducts;
using AeroMes.Domain.Master;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Constants;

// substitute materials
[JsonSerializable(typeof(SubstituteMaterialCreatedResult))]
[JsonSerializable(typeof(SubstituteMaterialDto))]
[JsonSerializable(typeof(IReadOnlyList<SubstituteMaterialDto>))]
[JsonSerializable(typeof(CreateSubstituteMaterialRequest))]
[JsonSerializable(typeof(UpdateSubstituteMaterialRequest))]
[JsonSerializable(typeof(SetSubstituteMaterialStatusRequest))]
// disassembly boms
[JsonSerializable(typeof(DisassemblyBomCreatedResult))]
[JsonSerializable(typeof(DisassemblyBomSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<DisassemblyBomSummaryDto>))]
[JsonSerializable(typeof(DisassemblyBomDetailDto))]
[JsonSerializable(typeof(DisassemblyBomLineDto))]
[JsonSerializable(typeof(IReadOnlyList<DisassemblyBomLineDto>))]
[JsonSerializable(typeof(CreateDisassemblyBomRequest))]
[JsonSerializable(typeof(UpdateDisassemblyBomRequest))]
[JsonSerializable(typeof(SetDisassemblyBomStatusRequest))]
[JsonSerializable(typeof(UpdateBomByProductsRequest))]
[JsonSerializable(typeof(BomByProductInput))]
[JsonSerializable(typeof(BomType))]
[JsonSerializable(typeof(DisassemblyBomType))]
[JsonSerializable(typeof(DisassemblyComponentType))]
[JsonSerializable(typeof(SubstituteMaterialStatus))]
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
[JsonSerializable(typeof(UpdateMachineTypeRequest))]
[JsonSerializable(typeof(UpdateMachineAttributesRequest))]
[JsonSerializable(typeof(IReadOnlyList<MachinesByTypeDto>))]
[JsonSerializable(typeof(IReadOnlyList<CompatibleMachineDto>))]
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
[JsonSerializable(typeof(UpdateTrackingRequest))]
[JsonSerializable(typeof(UpdateCustomAttributesRequest))]
[JsonSerializable(typeof(IReadOnlyList<ProductUoMConversionDto>))]
[JsonSerializable(typeof(AddUoMConversionRequest))]
[JsonSerializable(typeof(UpdateUoMConversionRequest))]
[JsonSerializable(typeof(UoMConversionCreatedResult))]
// inventory extensions
[JsonSerializable(typeof(IReadOnlyList<AvailableStockDto>))]
[JsonSerializable(typeof(IReadOnlyList<ExpiringStockDto>))]
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
// integration — batch production orders
[JsonSerializable(typeof(BatchOrderItem))]
[JsonSerializable(typeof(BatchCreateResult))]
[JsonSerializable(typeof(BatchOrderItemRequest))]
[JsonSerializable(typeof(BatchCreateProductionOrdersRequest))]
[JsonSerializable(typeof(IReadOnlyList<BatchOrderItemRequest>))]
// integration — multi-product production orders
[JsonSerializable(typeof(MultiProductionOrderCreatedResult))]
[JsonSerializable(typeof(MultiProductionOrderSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<MultiProductionOrderSummaryDto>))]
[JsonSerializable(typeof(MultiProductionOrderDetailDto))]
[JsonSerializable(typeof(MultiProductionOrderLineDto))]
[JsonSerializable(typeof(IReadOnlyList<MultiProductionOrderLineDto>))]
[JsonSerializable(typeof(CreateMpoLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<CreateMpoLineRequest>))]
[JsonSerializable(typeof(CreateMultiProductionOrderRequest))]
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
// stage handovers
[JsonSerializable(typeof(IReadOnlyList<HandoverFormSummaryDto>))]
[JsonSerializable(typeof(HandoverFormDetailDto))]
[JsonSerializable(typeof(HandoverLineDto))]
[JsonSerializable(typeof(IReadOnlyList<HandoverLineDto>))]
[JsonSerializable(typeof(CreateHandoverFormRequest))]
[JsonSerializable(typeof(HandoverLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<HandoverLineRequest>))]
[JsonSerializable(typeof(HandoverFormCreatedResult))]
// variant groups (product families)
[JsonSerializable(typeof(IReadOnlyList<ProductFamilySummaryDto>))]
[JsonSerializable(typeof(VariantMatrixDto))]
[JsonSerializable(typeof(DimensionDto))]
[JsonSerializable(typeof(IReadOnlyList<DimensionDto>))]
[JsonSerializable(typeof(DimensionValueDto))]
[JsonSerializable(typeof(IReadOnlyList<DimensionValueDto>))]
[JsonSerializable(typeof(VariantRowDto))]
[JsonSerializable(typeof(IReadOnlyList<VariantRowDto>))]
[JsonSerializable(typeof(VariantResolvedDto))]
[JsonSerializable(typeof(CreateProductFamilyRequest))]
[JsonSerializable(typeof(AddDimensionRequest))]
[JsonSerializable(typeof(AddDimensionValueRequest))]
[JsonSerializable(typeof(GenerateVariantRequest))]
[JsonSerializable(typeof(FamilyCreatedResult))]
[JsonSerializable(typeof(DimensionCreatedResult))]
[JsonSerializable(typeof(DimensionValueCreatedResult))]
[JsonSerializable(typeof(GenerateVariantResult))]
// machine additions
[JsonSerializable(typeof(DuplicateMachineRequest))]
// beginning inventory
[JsonSerializable(typeof(IReadOnlyList<BeginningInventoryEntryDto>))]
[JsonSerializable(typeof(BeginningInventoryEntryCreatedResult))]
[JsonSerializable(typeof(CreateBeginningInventoryEntryRequest))]
[JsonSerializable(typeof(UpdateBeginningInventoryEntryRequest))]
// factory warehouse receipts
[JsonSerializable(typeof(IReadOnlyList<FactoryWarehouseReceiptSummaryDto>))]
[JsonSerializable(typeof(FactoryWarehouseReceiptDetailDto))]
[JsonSerializable(typeof(FactoryWarehouseReceiptCreatedResult))]
[JsonSerializable(typeof(CreateFactoryWarehouseReceiptRequest))]
[JsonSerializable(typeof(UpdateFactoryWarehouseReceiptRequest))]
[JsonSerializable(typeof(IReadOnlyList<FactoryReceiptLineDto>))]
// factory warehouse exports
[JsonSerializable(typeof(IReadOnlyList<FactoryWarehouseExportSummaryDto>))]
[JsonSerializable(typeof(FactoryWarehouseExportDetailDto))]
[JsonSerializable(typeof(FactoryWarehouseExportCreatedResult))]
[JsonSerializable(typeof(CreateFactoryWarehouseExportRequest))]
[JsonSerializable(typeof(UpdateFactoryWarehouseExportRequest))]
[JsonSerializable(typeof(IReadOnlyList<FactoryExportLineDto>))]
// material transfer slips
[JsonSerializable(typeof(IReadOnlyList<MaterialTransferSlipSummaryDto>))]
[JsonSerializable(typeof(MaterialTransferSlipDetailDto))]
[JsonSerializable(typeof(MaterialTransferSlipCreatedResult))]
[JsonSerializable(typeof(CreateMaterialTransferSlipRequest))]
[JsonSerializable(typeof(UpdateMaterialTransferSlipRequest))]
[JsonSerializable(typeof(IReadOnlyList<MaterialTransferLineDto>))]
// stock policy & replenishment
[JsonSerializable(typeof(IReadOnlyList<StockPolicyDto>))]
[JsonSerializable(typeof(StockPolicyCreatedResult))]
[JsonSerializable(typeof(CreateStockPolicyRequest))]
[JsonSerializable(typeof(UpdateStockPolicyRequest))]
[JsonSerializable(typeof(IReadOnlyList<ReplenishmentAlertDto>))]
[JsonSerializable(typeof(IReadOnlyList<StockStatusItemDto>))]
// cycle count
[JsonSerializable(typeof(IReadOnlyList<CycleCountPlanSummaryDto>))]
[JsonSerializable(typeof(CycleCountPlanDetailDto))]
[JsonSerializable(typeof(CycleCountPlanCreatedResult))]
[JsonSerializable(typeof(CreateCycleCountPlanRequest))]
[JsonSerializable(typeof(GenerateCycleCountLinesRequest))]
[JsonSerializable(typeof(RecordCycleCountLineRequest))]
[JsonSerializable(typeof(ApproveCycleCountRequest))]
[JsonSerializable(typeof(ApproveCycleCountResult))]
[JsonSerializable(typeof(IReadOnlyList<CycleCountLineDto>))]
[JsonSerializable(typeof(IReadOnlyList<CycleCountSheetLineDto>))]
// finished product intake requests
[JsonSerializable(typeof(IReadOnlyList<FinishedProductIntakeRequestSummaryDto>))]
[JsonSerializable(typeof(FinishedProductIntakeRequestDetailDto))]
[JsonSerializable(typeof(FinishedProductIntakeRequestCreatedResult))]
[JsonSerializable(typeof(CreateFinishedProductIntakeRequestRequest))]
[JsonSerializable(typeof(UpdateFinishedProductIntakeRequestRequest))]
[JsonSerializable(typeof(ReceiveFinishedProductIntakeRequestRequest))]
[JsonSerializable(typeof(IReadOnlyList<IntakeRequestLineDto>))]
// material requisitions
[JsonSerializable(typeof(IReadOnlyList<MaterialRequisitionSummaryDto>))]
[JsonSerializable(typeof(MaterialRequisitionDetailDto))]
[JsonSerializable(typeof(MaterialRequisitionCreatedResult))]
[JsonSerializable(typeof(CreateMaterialRequisitionRequest))]
[JsonSerializable(typeof(UpdateMaterialRequisitionRequest))]
[JsonSerializable(typeof(FulfillMaterialRequisitionRequest))]
[JsonSerializable(typeof(IReadOnlyList<MaterialRequisitionLineDto>))]
// material supply requests
[JsonSerializable(typeof(IReadOnlyList<MaterialSupplyRequestSummaryDto>))]
[JsonSerializable(typeof(MaterialSupplyRequestDetailDto))]
[JsonSerializable(typeof(MaterialSupplyRequestCreatedResult))]
[JsonSerializable(typeof(CreateMaterialSupplyRequestRequest))]
[JsonSerializable(typeof(UpdateMaterialSupplyRequestRequest))]
[JsonSerializable(typeof(IReadOnlyList<MaterialSupplyRequestLineDto>))]
// lot allocation
[JsonSerializable(typeof(AllocationResult))]
[JsonSerializable(typeof(IReadOnlyList<LotAllocation>))]
[JsonSerializable(typeof(IReadOnlyList<RejectedLot>))]
[JsonSerializable(typeof(UpdateProductPickingConfigRequest))]
// RMA (returns)
[JsonSerializable(typeof(IReadOnlyList<RmaSummaryDto>))]
[JsonSerializable(typeof(RmaDetailDto))]
[JsonSerializable(typeof(RmaCreatedResult))]
[JsonSerializable(typeof(RmaLineAddedResult))]
[JsonSerializable(typeof(CreateRmaRequest))]
[JsonSerializable(typeof(AddRmaLineRequest))]
[JsonSerializable(typeof(ReceiveRmaRequest))]
[JsonSerializable(typeof(DisposeRmaLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<RmaLineDto>))]
[JsonSerializable(typeof(IReadOnlyList<RmaLineReceiptEntry>))]
// shipments, pick lists & cartons
[JsonSerializable(typeof(IReadOnlyList<ShipmentSummaryDto>))]
[JsonSerializable(typeof(ShipmentDetailDto))]
[JsonSerializable(typeof(ShipmentCreatedResult))]
[JsonSerializable(typeof(PickListCreatedResult))]
[JsonSerializable(typeof(PickListDetailDto))]
[JsonSerializable(typeof(CartonCreatedResult))]
[JsonSerializable(typeof(CreateShipmentRequest))]
[JsonSerializable(typeof(AddShipmentLineRequest))]
[JsonSerializable(typeof(CreatePickListRequest))]
[JsonSerializable(typeof(ConfirmPickLineRequest))]
[JsonSerializable(typeof(AddCartonContentRequest))]
[JsonSerializable(typeof(SealCartonRequest))]
[JsonSerializable(typeof(DispatchShipmentRequest))]
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
// inspection orders
[JsonSerializable(typeof(IReadOnlyList<InspectionOrderListDto>))]
[JsonSerializable(typeof(InspectionOrderDetailDto))]
[JsonSerializable(typeof(AssignInspectionOrderRequest))]
[JsonSerializable(typeof(WaiveInspectionOrderRequest))]
// inspection results
[JsonSerializable(typeof(InspectionResultDto))]
[JsonSerializable(typeof(IReadOnlyList<InspectionResultDto>))]
[JsonSerializable(typeof(RecordResultItem))]
[JsonSerializable(typeof(IReadOnlyList<RecordResultItem>))]
[JsonSerializable(typeof(RecordInspectionResultRequest))]
[JsonSerializable(typeof(BulkRecordInspectionResultsRequest))]
// inspection plans
[JsonSerializable(typeof(IReadOnlyList<InspectionPlanListDto>))]
[JsonSerializable(typeof(InspectionPlanDetailDto))]
[JsonSerializable(typeof(InspectionCharacteristicDto))]
[JsonSerializable(typeof(IReadOnlyList<InspectionCharacteristicDto>))]
[JsonSerializable(typeof(InspectionPlanCreatedResult))]
[JsonSerializable(typeof(CharacteristicCreatedResult))]
[JsonSerializable(typeof(CreateInspectionPlanRequest))]
[JsonSerializable(typeof(UpdateInspectionPlanRequest))]
[JsonSerializable(typeof(AddCharacteristicRequest))]
[JsonSerializable(typeof(UpdateCharacteristicRequest))]
[JsonSerializable(typeof(ReorderCharacteristicsRequest))]
// ncr
[JsonSerializable(typeof(IReadOnlyList<NcrListDto>))]
[JsonSerializable(typeof(NcrDetailDto))]
[JsonSerializable(typeof(NcrDefectLineDto))]
[JsonSerializable(typeof(IReadOnlyList<NcrDefectLineDto>))]
[JsonSerializable(typeof(NcrCreatedResult))]
[JsonSerializable(typeof(CreateManualNcrRequest))]
[JsonSerializable(typeof(SetNcrDispositionRequest))]
[JsonSerializable(typeof(CloseNcrRequest))]
[JsonSerializable(typeof(CancelNcrRequest))]
[JsonSerializable(typeof(UpdateNcrRequest))]
// iot
[JsonSerializable(typeof(AdapterDto))]
[JsonSerializable(typeof(AdapterDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<AdapterDto>))]
[JsonSerializable(typeof(SignalDto))]
[JsonSerializable(typeof(IReadOnlyList<SignalDto>))]
[JsonSerializable(typeof(StateRuleDto))]
[JsonSerializable(typeof(IReadOnlyList<StateRuleDto>))]
[JsonSerializable(typeof(AdapterCreatedResult))]
[JsonSerializable(typeof(SignalCreatedResult))]
[JsonSerializable(typeof(StateRuleCreatedResult))]
// iot signal tags
[JsonSerializable(typeof(SignalTagDto))]
[JsonSerializable(typeof(IReadOnlyList<SignalTagDto>))]
[JsonSerializable(typeof(CreateSignalTagRequest))]
[JsonSerializable(typeof(UpdateSignalTagRequest))]
// iot pipeline
[JsonSerializable(typeof(PipelineStats))]
// iot mqtt
[JsonSerializable(typeof(MqttConnectionTestResult))]
// iot opc-ua
[JsonSerializable(typeof(OpcBrowseRequest))]
[JsonSerializable(typeof(OpcNodeInfo))]
[JsonSerializable(typeof(IReadOnlyList<OpcNodeInfo>))]
[JsonSerializable(typeof(OpcReadNodeRequest))]
[JsonSerializable(typeof(OpcNodeValue))]
// iot modbus
[JsonSerializable(typeof(ModbusReadRegistersRequest))]
[JsonSerializable(typeof(ModbusRegisterValue))]
[JsonSerializable(typeof(ModbusReadRegistersResponse))]
[JsonSerializable(typeof(List<ModbusRegisterValue>))]
// iot webhook ingest
[JsonSerializable(typeof(WebhookSignalPayload))]
[JsonSerializable(typeof(IReadOnlyList<WebhookSignalPayload>))]
[JsonSerializable(typeof(WebhookMachineBundlePayload))]
[JsonSerializable(typeof(WebhookIngestResponse))]
[JsonSerializable(typeof(WebhookIngestError))]
[JsonSerializable(typeof(IReadOnlyList<WebhookIngestError>))]
[JsonSerializable(typeof(Dictionary<string, decimal>))]
// iot machine state engine
[JsonSerializable(typeof(MachineStateSnapshotDto))]
[JsonSerializable(typeof(IReadOnlyList<MachineStateSnapshotDto>))]
[JsonSerializable(typeof(MachineStateHistoryDto))]
[JsonSerializable(typeof(IReadOnlyList<MachineStateHistoryDto>))]
[JsonSerializable(typeof(StateOverrideRequest))]
// iot time-series storage & aggregation
[JsonSerializable(typeof(SignalHistoryPoint))]
[JsonSerializable(typeof(IReadOnlyList<SignalHistoryPoint>))]
[JsonSerializable(typeof(SignalAggPoint))]
[JsonSerializable(typeof(IReadOnlyList<SignalAggPoint>))]
[JsonSerializable(typeof(RetentionPolicyDto))]
[JsonSerializable(typeof(UpdateRetentionRequest))]
// storage stats
[JsonSerializable(typeof(TableStatsDto))]
[JsonSerializable(typeof(IotStorageStatsDto))]
// live signals
[JsonSerializable(typeof(LiveSignalDto))]
[JsonSerializable(typeof(IReadOnlyList<LiveSignalDto>))]
// adapter health
[JsonSerializable(typeof(AdapterHealthDto))]
[JsonSerializable(typeof(IReadOnlyList<AdapterHealthDto>))]
[JsonSerializable(typeof(AdapterHealthLogDto))]
[JsonSerializable(typeof(List<AdapterHealthLogDto>))]
[JsonSerializable(typeof(AdapterHealthDetailDto))]
// rules engine
[JsonSerializable(typeof(RuleListItemDto))]
[JsonSerializable(typeof(IReadOnlyList<RuleListItemDto>))]
[JsonSerializable(typeof(RuleDetailDto))]
[JsonSerializable(typeof(RuleConditionDto))]
[JsonSerializable(typeof(IReadOnlyList<RuleConditionDto>))]
[JsonSerializable(typeof(RuleActionDto))]
[JsonSerializable(typeof(IReadOnlyList<RuleActionDto>))]
[JsonSerializable(typeof(RuleExecutionLogDto))]
[JsonSerializable(typeof(IReadOnlyList<RuleExecutionLogDto>))]
[JsonSerializable(typeof(RuleCreatedResult))]
[JsonSerializable(typeof(ToggleRuleRequest))]
[JsonSerializable(typeof(RuleTestRequest))]
[JsonSerializable(typeof(RuleTestResult))]
[JsonSerializable(typeof(CreateRuleRequest))]
[JsonSerializable(typeof(UpdateRuleRequest))]
[JsonSerializable(typeof(ConditionSpec))]
[JsonSerializable(typeof(IReadOnlyList<ConditionSpec>))]
[JsonSerializable(typeof(ActionSpec))]
[JsonSerializable(typeof(IReadOnlyList<ActionSpec>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
// sop
[JsonSerializable(typeof(SopDocumentListDto))]
[JsonSerializable(typeof(List<SopDocumentListDto>))]
[JsonSerializable(typeof(SopDocumentDetailDto))]
[JsonSerializable(typeof(CheckItemDto))]
[JsonSerializable(typeof(List<CheckItemDto>))]
[JsonSerializable(typeof(ChecksheetInstanceDto))]
[JsonSerializable(typeof(List<ChecksheetInstanceDto>))]
[JsonSerializable(typeof(CheckItemResultDto))]
[JsonSerializable(typeof(List<CheckItemResultDto>))]
[JsonSerializable(typeof(CheckItemSpec))]
[JsonSerializable(typeof(List<CheckItemSpec>))]
[JsonSerializable(typeof(CreateSopDocumentRequest))]
[JsonSerializable(typeof(SetSopStatusRequest))]
[JsonSerializable(typeof(RecordItemResultRequest))]
[JsonSerializable(typeof(OverrideInstanceRequest))]
// lab
[JsonSerializable(typeof(TestMethodDto))]
[JsonSerializable(typeof(List<TestMethodDto>))]
[JsonSerializable(typeof(TestPanelDto))]
[JsonSerializable(typeof(List<TestPanelDto>))]
[JsonSerializable(typeof(TestPanelItemDto))]
[JsonSerializable(typeof(List<TestPanelItemDto>))]
[JsonSerializable(typeof(LabRequestListDto))]
[JsonSerializable(typeof(List<LabRequestListDto>))]
[JsonSerializable(typeof(LabRequestDetailDto))]
[JsonSerializable(typeof(LabSampleDto))]
[JsonSerializable(typeof(List<LabSampleDto>))]
[JsonSerializable(typeof(LabResultDto))]
[JsonSerializable(typeof(List<LabResultDto>))]
[JsonSerializable(typeof(LabReportDto))]
[JsonSerializable(typeof(UpsertMethodRequest))]
[JsonSerializable(typeof(PanelItemSpec))]
[JsonSerializable(typeof(List<PanelItemSpec>))]
[JsonSerializable(typeof(UpsertPanelRequest))]
[JsonSerializable(typeof(CreateRequestDto))]
[JsonSerializable(typeof(AssignRequestDto))]
[JsonSerializable(typeof(AddSampleDto))]
[JsonSerializable(typeof(DisposeSampleDto))]
[JsonSerializable(typeof(UpsertResultDto))]
[JsonSerializable(typeof(List<UpsertResultDto>))]
[JsonSerializable(typeof(IssueReportDto))]
[JsonSerializable(typeof(QrTextRequest))]
// labels
[JsonSerializable(typeof(LabelTemplateDto))]
[JsonSerializable(typeof(List<LabelTemplateDto>))]
[JsonSerializable(typeof(UpsertTemplateRequest))]
[JsonSerializable(typeof(LabelPrintJobDto))]
[JsonSerializable(typeof(List<LabelPrintJobDto>))]
[JsonSerializable(typeof(CreatePrintJobRequest))]
[JsonSerializable(typeof(ScanResultDto))]
[JsonSerializable(typeof(BatchLabelItem))]
[JsonSerializable(typeof(BatchLabelItem[]))]
[JsonSerializable(typeof(BatchLabelRequest))]
[JsonSerializable(typeof(BatchLabelResponse))]
[JsonSerializable(typeof(LabelEncodeRequest))]
[JsonSerializable(typeof(LabelDecodeResponse))]
// reminders
[JsonSerializable(typeof(ReminderAlertDto))]
[JsonSerializable(typeof(List<ReminderAlertDto>))]
[JsonSerializable(typeof(UnreadCountDto))]
[JsonSerializable(typeof(ReminderConfigDto))]
[JsonSerializable(typeof(List<ReminderConfigDto>))]
[JsonSerializable(typeof(UpsertReminderConfigRequest))]
// defect lifecycle
[JsonSerializable(typeof(DefectEntryDto))]
[JsonSerializable(typeof(List<DefectEntryDto>))]
[JsonSerializable(typeof(RepairOrderDto))]
[JsonSerializable(typeof(List<RepairOrderDto>))]
[JsonSerializable(typeof(RepairMaterialLineDto))]
[JsonSerializable(typeof(List<RepairMaterialLineDto>))]
[JsonSerializable(typeof(CreateDefectEntryRequest))]
[JsonSerializable(typeof(SetDefectEntryStatusRequest))]
[JsonSerializable(typeof(CreateRepairOrderRequest))]
[JsonSerializable(typeof(CreateRepairMaterialLineRequest))]
[JsonSerializable(typeof(List<CreateRepairMaterialLineRequest>))]
[JsonSerializable(typeof(SetRepairOrderStatusRequest))]
// file storage
[JsonSerializable(typeof(FileUploadResult))]
[JsonSerializable(typeof(FileObjectDto))]
[JsonSerializable(typeof(IReadOnlyList<FileObjectDto>))]
// localization
[JsonSerializable(typeof(SupportedLanguageDto))]
[JsonSerializable(typeof(List<SupportedLanguageDto>))]
[JsonSerializable(typeof(ApiResponse<List<SupportedLanguageDto>>))]
public partial class ApiJsonContext : JsonSerializerContext
{

}
