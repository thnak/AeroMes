using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Application.Interfaces;
using AeroMes.Application.Production.Schedule.Commands.UpdateScheduleLines;
using AeroMes.Domain.Production.Repositories;
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
using AeroMes.Application.Production.Queries.GetProductionOrderProgress;
using AeroMes.Application.Production.Statistics.Commands.CreateProductionStatisticsSheet;
using AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheetDetail;
using AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheets;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Application.Traceability.Services;
using AeroMes.Application.Traceability.Commands.CommissionSerialUnits;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Application.Production.Commands.CreateProductionPlan;
using AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;
using AeroMes.Application.Production.Commands.CreateMaterialPurchaseRequest;
using AeroMes.Application.Quality.InspectionVouchers.Commands.CreateInspectionVoucher;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Maintenance.Repositories;
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
using AeroMes.Application.Cost.WOCosts.Queries.GetWOCostBreakdown;
using AeroMes.Application.Production.MRP.Commands.UpdateMrp;
using AeroMes.Domain.Production;
using AeroMes.Domain.Master;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Domain.Energy;
using AeroMes.Domain.Energy.Repositories;
using AeroMes.Application.Templates;
using AeroMes.Application.Documents.Queries.GetDocumentPrintTemplates;
using AeroMes.Domain.Templates;
using AeroMes.Application.Import;
using AeroMes.Application.Import.Commands.ValidateImport;
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
[JsonSerializable(typeof(LotHoldProblemResponse))]
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
// traceability
[JsonSerializable(typeof(LotGenealogyDto))]
[JsonSerializable(typeof(LineageNodeDto))]
[JsonSerializable(typeof(IReadOnlyList<LineageNodeDto>))]
[JsonSerializable(typeof(LineageEdgeDto))]
[JsonSerializable(typeof(IReadOnlyList<LineageEdgeDto>))]
[JsonSerializable(typeof(LotEventDto))]
[JsonSerializable(typeof(IReadOnlyList<LotEventDto>))]
[JsonSerializable(typeof(RecordLineageRequest))]
[JsonSerializable(typeof(AppendLotEventRequest))]
// recalls
[JsonSerializable(typeof(RecallDetailDto))]
[JsonSerializable(typeof(RecallSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<RecallSummaryDto>))]
[JsonSerializable(typeof(PagedResult<RecallSummaryDto>))]
[JsonSerializable(typeof(RecallScopeDto))]
[JsonSerializable(typeof(RecallScopeLotDto))]
[JsonSerializable(typeof(IReadOnlyList<RecallScopeLotDto>))]
[JsonSerializable(typeof(RecallAuditEntryDto))]
[JsonSerializable(typeof(IReadOnlyList<RecallAuditEntryDto>))]
[JsonSerializable(typeof(RecallAuditReportDto))]
[JsonSerializable(typeof(RecallQuarantineResultDto))]
[JsonSerializable(typeof(InitiateRecallRequest))]
[JsonSerializable(typeof(CloseRecallRequest))]
// lot holds
[JsonSerializable(typeof(LotHoldDto))]
[JsonSerializable(typeof(IReadOnlyList<LotHoldDto>))]
[JsonSerializable(typeof(LotHoldStatusDto))]
[JsonSerializable(typeof(BulkHoldResultDto))]
[JsonSerializable(typeof(PagedResult<LotHoldDto>))]
[JsonSerializable(typeof(PlaceHoldRequest))]
[JsonSerializable(typeof(ReleaseHoldRequest))]
[JsonSerializable(typeof(RejectDispositionRequest))]
[JsonSerializable(typeof(BulkHoldFromForwardTraceRequest))]
// process records (as-built & mid-session WIP)
[JsonSerializable(typeof(ProcessRecordDto))]
[JsonSerializable(typeof(IReadOnlyList<ProcessRecordDto>))]
[JsonSerializable(typeof(ProcessRecordDetailDto))]
[JsonSerializable(typeof(ProcessParameterDto))]
[JsonSerializable(typeof(IReadOnlyList<ProcessParameterDto>))]
[JsonSerializable(typeof(OpenProcessRecordRequest))]
[JsonSerializable(typeof(RecordParameterRequest))]
[JsonSerializable(typeof(CloseProcessRecordRequest))]
// production statistics sheets
[JsonSerializable(typeof(StatisticsSheetCreatedResult))]
[JsonSerializable(typeof(ProductionStatisticsSheetSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionStatisticsSheetSummaryDto>))]
[JsonSerializable(typeof(ProductionStatisticsSheetDetailDto))]
[JsonSerializable(typeof(OutputLineDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<OutputLineDetailDto>))]
[JsonSerializable(typeof(MaterialLineDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<MaterialLineDetailDto>))]
[JsonSerializable(typeof(ByProductLineDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ByProductLineDetailDto>))]
[JsonSerializable(typeof(CreateProductionStatisticsSheetRequest))]
[JsonSerializable(typeof(OutputLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<OutputLineRequest>))]
[JsonSerializable(typeof(MaterialLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<MaterialLineRequest>))]
[JsonSerializable(typeof(ByProductLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<ByProductLineRequest>))]
// production order progress
[JsonSerializable(typeof(ProductionOrderProgressDto))]
[JsonSerializable(typeof(StageProgressDto))]
[JsonSerializable(typeof(IReadOnlyList<StageProgressDto>))]
[JsonSerializable(typeof(MaterialProgressDto))]
[JsonSerializable(typeof(IReadOnlyList<MaterialProgressDto>))]
[JsonSerializable(typeof(OutputSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<OutputSummaryDto>))]
[JsonSerializable(typeof(QualityProgressDto))]
[JsonSerializable(typeof(IReadOnlyList<QualityProgressDto>))]
[JsonSerializable(typeof(DefectSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<DefectSummaryDto>))]
[JsonSerializable(typeof(HandoverProgressDto))]
[JsonSerializable(typeof(IReadOnlyList<HandoverProgressDto>))]
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
[JsonSerializable(typeof(PoMaterialLineDto))]
[JsonSerializable(typeof(IReadOnlyList<PoMaterialLineDto>))]
[JsonSerializable(typeof(PoStageDto))]
[JsonSerializable(typeof(IReadOnlyList<PoStageDto>))]
[JsonSerializable(typeof(CreateProductionOrderRequest))]
[JsonSerializable(typeof(UpdatePoStatusRequest))]
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
// mold management extensions
[JsonSerializable(typeof(SetCompatibilityRequest))]
[JsonSerializable(typeof(AssignMoldToJobRequest))]
[JsonSerializable(typeof(IncrementShotsRequest))]
[JsonSerializable(typeof(MoldCompatibilityDto))]
[JsonSerializable(typeof(IReadOnlyList<MoldCompatibilityDto>))]
[JsonSerializable(typeof(List<MoldCompatibilityDto>))]
[JsonSerializable(typeof(MoldAssignmentDto))]
[JsonSerializable(typeof(IReadOnlyList<MoldAssignmentDto>))]
[JsonSerializable(typeof(List<MoldAssignmentDto>))]
// serial unit traceability
[JsonSerializable(typeof(CommissionSerialUnitsRequest))]
[JsonSerializable(typeof(PackSerialsRequest))]
[JsonSerializable(typeof(UnpackSerialsRequest))]
[JsonSerializable(typeof(ShipSerialRequest))]
[JsonSerializable(typeof(RecallSerialRequest))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(SerialUnitDto))]
[JsonSerializable(typeof(IReadOnlyList<SerialUnitDto>))]
[JsonSerializable(typeof(List<SerialUnitDto>))]
[JsonSerializable(typeof(PagedResult<SerialUnitDto>))]
[JsonSerializable(typeof(SerialUnitDetailDto))]
[JsonSerializable(typeof(SerialLotLineageDto))]
[JsonSerializable(typeof(IReadOnlyList<SerialLotLineageDto>))]
[JsonSerializable(typeof(List<SerialLotLineageDto>))]
[JsonSerializable(typeof(SerialEventDto))]
[JsonSerializable(typeof(IReadOnlyList<SerialEventDto>))]
[JsonSerializable(typeof(List<SerialEventDto>))]
[JsonSerializable(typeof(SSCCContentsDto))]
[JsonSerializable(typeof(LotConsumptionDto))]
[JsonSerializable(typeof(IReadOnlyList<LotConsumptionDto>))]
[JsonSerializable(typeof(List<LotConsumptionDto>))]
// bundle WIP tracking (apparel)
[JsonSerializable(typeof(CreateBundlesFromCutOrderRequest))]
[JsonSerializable(typeof(ReceiveBundleRequest))]
[JsonSerializable(typeof(CloseBundleRequest))]
[JsonSerializable(typeof(ReworkBundleRequest))]
[JsonSerializable(typeof(BundleLocationDto))]
[JsonSerializable(typeof(WIPByStyleDto))]
[JsonSerializable(typeof(IReadOnlyList<WIPByStyleDto>))]
[JsonSerializable(typeof(List<WIPByStyleDto>))]
[JsonSerializable(typeof(LineBalancingDto))]
[JsonSerializable(typeof(IReadOnlyList<LineBalancingDto>))]
[JsonSerializable(typeof(List<LineBalancingDto>))]
[JsonSerializable(typeof(OperatorEfficiencyDto))]
[JsonSerializable(typeof(IReadOnlyList<OperatorEfficiencyDto>))]
[JsonSerializable(typeof(List<OperatorEfficiencyDto>))]
// cut order planning (apparel)
[JsonSerializable(typeof(CreateCutOrderRequest))]
[JsonSerializable(typeof(CutOrderLineRequestItem))]
[JsonSerializable(typeof(IReadOnlyList<CutOrderLineRequestItem>))]
[JsonSerializable(typeof(List<CutOrderLineRequestItem>))]
[JsonSerializable(typeof(ReserveFabricRequest))]
[JsonSerializable(typeof(StartCuttingRequest))]
[JsonSerializable(typeof(CompleteCuttingRequest))]
[JsonSerializable(typeof(CompleteCuttingLineRequestItem))]
[JsonSerializable(typeof(IReadOnlyList<CompleteCuttingLineRequestItem>))]
[JsonSerializable(typeof(List<CompleteCuttingLineRequestItem>))]
[JsonSerializable(typeof(CutOrderDto))]
[JsonSerializable(typeof(IReadOnlyList<CutOrderDto>))]
[JsonSerializable(typeof(List<CutOrderDto>))]
[JsonSerializable(typeof(CutOrderLineDto))]
[JsonSerializable(typeof(IReadOnlyList<CutOrderLineDto>))]
[JsonSerializable(typeof(CutOrderFabricUsageDto))]
[JsonSerializable(typeof(IReadOnlyList<CutOrderFabricUsageDto>))]
[JsonSerializable(typeof(MarkerEfficiencyReportDto))]
[JsonSerializable(typeof(IReadOnlyList<MarkerEfficiencyReportDto>))]
[JsonSerializable(typeof(List<MarkerEfficiencyReportDto>))]
// fabric roll management (apparel)
[JsonSerializable(typeof(RegisterFabricRollRequest))]
[JsonSerializable(typeof(ReserveFabricRollsRequest))]
[JsonSerializable(typeof(ConsumeFabricRequest))]
[JsonSerializable(typeof(QuarantineFabricRollRequest))]
[JsonSerializable(typeof(FabricRollDto))]
[JsonSerializable(typeof(IReadOnlyList<FabricRollDto>))]
[JsonSerializable(typeof(List<FabricRollDto>))]
[JsonSerializable(typeof(FabricConsumptionLogDto))]
[JsonSerializable(typeof(IReadOnlyList<FabricConsumptionLogDto>))]
[JsonSerializable(typeof(List<FabricConsumptionLogDto>))]
[JsonSerializable(typeof(FabricInventorySummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<FabricInventorySummaryDto>))]
[JsonSerializable(typeof(List<FabricInventorySummaryDto>))]
// material blend / regrind tracking
[JsonSerializable(typeof(RecordBlendRequest))]
[JsonSerializable(typeof(ApproveBlendRequest))]
[JsonSerializable(typeof(MaterialBlendLogDto))]
[JsonSerializable(typeof(IReadOnlyList<MaterialBlendLogDto>))]
[JsonSerializable(typeof(List<MaterialBlendLogDto>))]
[JsonSerializable(typeof(PagedResult<MaterialBlendLogDto>))]
[JsonSerializable(typeof(RegrindUsageSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<RegrindUsageSummaryDto>))]
[JsonSerializable(typeof(List<RegrindUsageSummaryDto>))]
// disassembly order management — #132
[JsonSerializable(typeof(CreateDisassemblyOrderRequest))]
[JsonSerializable(typeof(UpdateStatusRequest))]
[JsonSerializable(typeof(RecordRecoveryRequest))]
[JsonSerializable(typeof(DisassemblyOrderDto))]
[JsonSerializable(typeof(PagedResult<DisassemblyOrderDto>))]
[JsonSerializable(typeof(DisassemblyRecoveredLineDto))]
[JsonSerializable(typeof(IReadOnlyList<DisassemblyRecoveredLineDto>))]
[JsonSerializable(typeof(List<DisassemblyRecoveredLineDto>))]
// packaging management — #131
[JsonSerializable(typeof(CreatePackagingBomRequest))]
[JsonSerializable(typeof(PackagingBomLineRequest))]
[JsonSerializable(typeof(IReadOnlyList<PackagingBomLineRequest>))]
[JsonSerializable(typeof(List<PackagingBomLineRequest>))]
[JsonSerializable(typeof(CreatePackagingOrderRequest))]
[JsonSerializable(typeof(RecordPackagedRequest))]
[JsonSerializable(typeof(PrintLabelRequest))]
[JsonSerializable(typeof(PackagingBomDto))]
[JsonSerializable(typeof(IReadOnlyList<PackagingBomDto>))]
[JsonSerializable(typeof(List<PackagingBomDto>))]
[JsonSerializable(typeof(PackagingBomLineDto))]
[JsonSerializable(typeof(IReadOnlyList<PackagingBomLineDto>))]
[JsonSerializable(typeof(PackagingOrderDto))]
[JsonSerializable(typeof(IReadOnlyList<PackagingOrderDto>))]
[JsonSerializable(typeof(List<PackagingOrderDto>))]
// DHU-based quality (apparel) — #128
[JsonSerializable(typeof(RecordInlineInspectionRequest))]
[JsonSerializable(typeof(InlineDefectRequestItem))]
[JsonSerializable(typeof(IReadOnlyList<InlineDefectRequestItem>))]
[JsonSerializable(typeof(List<InlineDefectRequestItem>))]
[JsonSerializable(typeof(RecordAQLInspectionRequest))]
[JsonSerializable(typeof(AQLDefectRequestItem))]
[JsonSerializable(typeof(IReadOnlyList<AQLDefectRequestItem>))]
[JsonSerializable(typeof(List<AQLDefectRequestItem>))]
[JsonSerializable(typeof(DHUTrendDto))]
[JsonSerializable(typeof(IReadOnlyList<DHUTrendDto>))]
[JsonSerializable(typeof(List<DHUTrendDto>))]
[JsonSerializable(typeof(DefectParetoDto))]
[JsonSerializable(typeof(IReadOnlyList<DefectParetoDto>))]
[JsonSerializable(typeof(List<DefectParetoDto>))]
[JsonSerializable(typeof(QualitySummaryByStyleDto))]
// material purchase request — #89
[JsonSerializable(typeof(CreatePurchaseRequestRequest))]
[JsonSerializable(typeof(PurchaseRequestLineInput))]
[JsonSerializable(typeof(IReadOnlyList<PurchaseRequestLineInput>))]
[JsonSerializable(typeof(List<PurchaseRequestLineInput>))]
[JsonSerializable(typeof(ApproveRequestBody))]
[JsonSerializable(typeof(MaterialPurchaseRequestDto))]
[JsonSerializable(typeof(PagedResult<MaterialPurchaseRequestDto>))]
[JsonSerializable(typeof(MaterialPurchaseRequestLineDto))]
[JsonSerializable(typeof(IReadOnlyList<MaterialPurchaseRequestLineDto>))]
[JsonSerializable(typeof(List<MaterialPurchaseRequestLineDto>))]
// sampling method catalog — #78
[JsonSerializable(typeof(CreateSamplingMethodRequest))]
[JsonSerializable(typeof(UpdateSamplingMethodRequest))]
[JsonSerializable(typeof(VolumeRangeRequest))]
[JsonSerializable(typeof(VolumeRangeInput))]
[JsonSerializable(typeof(IReadOnlyList<VolumeRangeInput>))]
[JsonSerializable(typeof(List<VolumeRangeInput>))]
[JsonSerializable(typeof(SamplingMethodDto))]
[JsonSerializable(typeof(IReadOnlyList<SamplingMethodDto>))]
[JsonSerializable(typeof(List<SamplingMethodDto>))]
[JsonSerializable(typeof(SamplingVolumeRangeDto))]
[JsonSerializable(typeof(IReadOnlyList<SamplingVolumeRangeDto>))]
[JsonSerializable(typeof(List<SamplingVolumeRangeDto>))]
// maintenance & repair cost — #90
[JsonSerializable(typeof(CreateMaintOrderRequest))]
[JsonSerializable(typeof(AddCostLineRequest))]
[JsonSerializable(typeof(UpdateMaintStatusRequest))]
[JsonSerializable(typeof(MaintenanceOrderDto))]
[JsonSerializable(typeof(PagedResult<MaintenanceOrderDto>))]
[JsonSerializable(typeof(MaintCostLineDto))]
[JsonSerializable(typeof(IReadOnlyList<MaintCostLineDto>))]
[JsonSerializable(typeof(List<MaintCostLineDto>))]
[JsonSerializable(typeof(MachineTcoDto))]
[JsonSerializable(typeof(IReadOnlyList<MachineTcoDto>))]
[JsonSerializable(typeof(List<MachineTcoDto>))]
// scrap & rework cost management — #104
[JsonSerializable(typeof(PostScrapRequest))]
[JsonSerializable(typeof(CreateReworkOrderRequest))]
[JsonSerializable(typeof(CloseReworkOrderRequest))]
[JsonSerializable(typeof(ScrapParetoDto))]
[JsonSerializable(typeof(IReadOnlyList<ScrapParetoDto>))]
[JsonSerializable(typeof(List<ScrapParetoDto>))]
[JsonSerializable(typeof(ScrapTransactionDto))]
[JsonSerializable(typeof(PagedResult<ScrapTransactionDto>))]
[JsonSerializable(typeof(ReworkOrderDto))]
[JsonSerializable(typeof(PagedResult<ReworkOrderDto>))]
[JsonSerializable(typeof(QualityCostSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<QualityCostSummaryDto>))]
[JsonSerializable(typeof(List<QualityCostSummaryDto>))]
[JsonSerializable(typeof(CopqTrendPointDto))]
[JsonSerializable(typeof(IReadOnlyList<CopqTrendPointDto>))]
[JsonSerializable(typeof(List<CopqTrendPointDto>))]
// quality criteria groups — #66
[JsonSerializable(typeof(CreateCriteriaGroupRequest))]
[JsonSerializable(typeof(UpdateCriteriaGroupRequest))]
[JsonSerializable(typeof(SetCriteriaGroupStatusRequest))]
[JsonSerializable(typeof(QualityCriteriaGroupDto))]
[JsonSerializable(typeof(IReadOnlyList<QualityCriteriaGroupDto>))]
[JsonSerializable(typeof(List<QualityCriteriaGroupDto>))]
[JsonSerializable(typeof(CriteriaGroupStatus))]
// quality inspection requests — #81 / #69
[JsonSerializable(typeof(CreateInspectionRequestRequest))]
[JsonSerializable(typeof(UpdateInspectionRequestRequest))]
[JsonSerializable(typeof(UpdateRequestStatusRequest))]
[JsonSerializable(typeof(InspectionRequestDto))]
[JsonSerializable(typeof(PagedResult<InspectionRequestDto>))]
[JsonSerializable(typeof(InspectionRequestDetailDto))]
[JsonSerializable(typeof(LinkedVoucherSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<LinkedVoucherSummaryDto>))]
[JsonSerializable(typeof(List<LinkedVoucherSummaryDto>))]
[JsonSerializable(typeof(InspectionPriority))]
// quality inspection vouchers — #92
[JsonSerializable(typeof(CreateInspectionVoucherRequest))]
[JsonSerializable(typeof(AddDefectRequest))]
[JsonSerializable(typeof(UpdateVoucherStatusRequest))]
[JsonSerializable(typeof(QualityInspectionVoucherDto))]
[JsonSerializable(typeof(PagedResult<QualityInspectionVoucherDto>))]
[JsonSerializable(typeof(VoucherDefectDto))]
[JsonSerializable(typeof(IReadOnlyList<VoucherDefectDto>))]
[JsonSerializable(typeof(List<VoucherDefectDto>))]
// energy & utility consumption monitoring — #99
[JsonSerializable(typeof(RegisterMeterRequest))]
[JsonSerializable(typeof(RegisterReadingRequest))]
[JsonSerializable(typeof(CloseShiftRequest))]
[JsonSerializable(typeof(MeterDto))]
[JsonSerializable(typeof(ShiftEnergyDto))]
[JsonSerializable(typeof(IReadOnlyList<ShiftEnergyDto>))]
[JsonSerializable(typeof(List<ShiftEnergyDto>))]
[JsonSerializable(typeof(EnergyIntensityTrendDto))]
[JsonSerializable(typeof(IReadOnlyList<EnergyIntensityTrendDto>))]
[JsonSerializable(typeof(List<EnergyIntensityTrendDto>))]
[JsonSerializable(typeof(UtilityType))]
[JsonSerializable(typeof(ReadingType))]
// production planning (order-based) — #135
[JsonSerializable(typeof(CreateProductionPlanRequest))]
[JsonSerializable(typeof(PlanLineInput))]
[JsonSerializable(typeof(IReadOnlyList<PlanLineInput>))]
[JsonSerializable(typeof(List<PlanLineInput>))]
[JsonSerializable(typeof(UpdatePlanStatusRequest))]
[JsonSerializable(typeof(UpdatePlanLineRequest))]
[JsonSerializable(typeof(ProductionPlanDto))]
[JsonSerializable(typeof(PagedResult<ProductionPlanDto>))]
[JsonSerializable(typeof(ProductionPlanLineDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionPlanLineDto>))]
[JsonSerializable(typeof(List<ProductionPlanLineDto>))]
[JsonSerializable(typeof(GanttTeamDto))]
[JsonSerializable(typeof(IReadOnlyList<GanttTeamDto>))]
[JsonSerializable(typeof(List<GanttTeamDto>))]
[JsonSerializable(typeof(GanttIntervalDto))]
[JsonSerializable(typeof(IReadOnlyList<GanttIntervalDto>))]
[JsonSerializable(typeof(List<GanttIntervalDto>))]
// quality standard set catalog — #95
[JsonSerializable(typeof(StandardSetStatus))]
[JsonSerializable(typeof(CreateStandardSetRequest))]
[JsonSerializable(typeof(UpdateStandardSetRequest))]
[JsonSerializable(typeof(SetStandardSetStatusRequest))]
[JsonSerializable(typeof(ProductCriteriaRequest))]
[JsonSerializable(typeof(StageCriteriaRequest))]
[JsonSerializable(typeof(IReadOnlyList<ProductCriteriaRequest>))]
[JsonSerializable(typeof(IReadOnlyList<StageCriteriaRequest>))]
[JsonSerializable(typeof(StandardSetListDto))]
[JsonSerializable(typeof(IReadOnlyList<StandardSetListDto>))]
[JsonSerializable(typeof(List<StandardSetListDto>))]
[JsonSerializable(typeof(PagedResult<StandardSetListDto>))]
[JsonSerializable(typeof(StandardSetDetailDto))]
[JsonSerializable(typeof(StandardSetCriteriaDto))]
[JsonSerializable(typeof(IReadOnlyList<StandardSetCriteriaDto>))]
[JsonSerializable(typeof(List<StandardSetCriteriaDto>))]
[JsonSerializable(typeof(StandardSetStageCriteriaDto))]
[JsonSerializable(typeof(IReadOnlyList<StandardSetStageCriteriaDto>))]
[JsonSerializable(typeof(List<StandardSetStageCriteriaDto>))]
// quality criteria catalog — #60
[JsonSerializable(typeof(CreateQualityCriteriaRequest))]
[JsonSerializable(typeof(UpdateQualityCriteriaRequest))]
[JsonSerializable(typeof(SetQualityCriteriaStatusRequest))]
[JsonSerializable(typeof(QualityCriteriaDto))]
[JsonSerializable(typeof(IReadOnlyList<QualityCriteriaDto>))]
[JsonSerializable(typeof(List<QualityCriteriaDto>))]
[JsonSerializable(typeof(CriteriaType))]
[JsonSerializable(typeof(CriteriaStatus))]
// MRP — #76
[JsonSerializable(typeof(MrpStatus))]
[JsonSerializable(typeof(CreateMrpRequest))]
[JsonSerializable(typeof(UpdateMrpRequest))]
[JsonSerializable(typeof(MrpLineInput))]
[JsonSerializable(typeof(IReadOnlyList<MrpLineInput>))]
[JsonSerializable(typeof(MrpListDto))]
[JsonSerializable(typeof(IReadOnlyList<MrpListDto>))]
[JsonSerializable(typeof(List<MrpListDto>))]
[JsonSerializable(typeof(PagedResult<MrpListDto>))]
[JsonSerializable(typeof(MrpDetailDto))]
[JsonSerializable(typeof(MrpLineDto))]
[JsonSerializable(typeof(IReadOnlyList<MrpLineDto>))]
[JsonSerializable(typeof(List<MrpLineDto>))]
// WO actual cost capture — #74
[JsonSerializable(typeof(PostMaterialCostRequest))]
[JsonSerializable(typeof(CloseJobCostRequest))]
[JsonSerializable(typeof(CloseWORequest))]
[JsonSerializable(typeof(WOCostSummaryDto))]
[JsonSerializable(typeof(WOMaterialCostLineDto))]
[JsonSerializable(typeof(IReadOnlyList<WOMaterialCostLineDto>))]
[JsonSerializable(typeof(List<WOMaterialCostLineDto>))]
[JsonSerializable(typeof(WOLaborCostLineDto))]
[JsonSerializable(typeof(IReadOnlyList<WOLaborCostLineDto>))]
[JsonSerializable(typeof(List<WOLaborCostLineDto>))]
[JsonSerializable(typeof(WOMachineCostLineDto))]
[JsonSerializable(typeof(IReadOnlyList<WOMachineCostLineDto>))]
[JsonSerializable(typeof(List<WOMachineCostLineDto>))]
[JsonSerializable(typeof(WOCostBreakdownResult))]
[JsonSerializable(typeof(VarianceReportItemDto))]
[JsonSerializable(typeof(IReadOnlyList<VarianceReportItemDto>))]
[JsonSerializable(typeof(List<VarianceReportItemDto>))]
[JsonSerializable(typeof(PagedResult<VarianceReportItemDto>))]
// cost rate master — #53
[JsonSerializable(typeof(UpsertLaborGradeRequest))]
[JsonSerializable(typeof(LaborGradeDto))]
[JsonSerializable(typeof(IReadOnlyList<LaborGradeDto>))]
[JsonSerializable(typeof(List<LaborGradeDto>))]
[JsonSerializable(typeof(UpsertMachineCostRateRequest))]
[JsonSerializable(typeof(MachineCostRateDto))]
[JsonSerializable(typeof(IReadOnlyList<MachineCostRateDto>))]
[JsonSerializable(typeof(List<MachineCostRateDto>))]
[JsonSerializable(typeof(MachineTotalRateDto))]
[JsonSerializable(typeof(CreateEnergyTariffRequest))]
[JsonSerializable(typeof(SetTariffStatusRequest))]
[JsonSerializable(typeof(EnergyTariffDto))]
[JsonSerializable(typeof(IReadOnlyList<EnergyTariffDto>))]
[JsonSerializable(typeof(List<EnergyTariffDto>))]
[JsonSerializable(typeof(UpsertMachineEnergyProfileRequest))]
[JsonSerializable(typeof(MachineEnergyProfileDto))]
[JsonSerializable(typeof(IReadOnlyList<MachineEnergyProfileDto>))]
[JsonSerializable(typeof(List<MachineEnergyProfileDto>))]
[JsonSerializable(typeof(SetItemStandardCostRequest))]
[JsonSerializable(typeof(ItemCostHistoryDto))]
[JsonSerializable(typeof(IReadOnlyList<ItemCostHistoryDto>))]
[JsonSerializable(typeof(List<ItemCostHistoryDto>))]
[JsonSerializable(typeof(MachineCostRateType))]
[JsonSerializable(typeof(EnergyTariffType))]
[JsonSerializable(typeof(ItemCostType))]
// production process step catalog — #57
[JsonSerializable(typeof(CreateProcessStepRequest))]
[JsonSerializable(typeof(UpdateProcessStepRequest))]
[JsonSerializable(typeof(SetStepStatusRequest))]
[JsonSerializable(typeof(DuplicateProcessStepRequest))]
[JsonSerializable(typeof(ProductionProcessStepDto))]
[JsonSerializable(typeof(PagedResult<ProductionProcessStepDto>))]
// production process catalog — #71
[JsonSerializable(typeof(CreateProductionProcessRequest))]
[JsonSerializable(typeof(StageInputRequest))]
[JsonSerializable(typeof(IReadOnlyList<StageInputRequest>))]
[JsonSerializable(typeof(List<StageInputRequest>))]
[JsonSerializable(typeof(UpdateProductionProcessRequest))]
[JsonSerializable(typeof(SetProcessStatusRequest))]
[JsonSerializable(typeof(ProductionProcessListDto))]
[JsonSerializable(typeof(PagedResult<ProductionProcessListDto>))]
[JsonSerializable(typeof(ProductionProcessDetailDto))]
[JsonSerializable(typeof(ProductionProcessStageDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionProcessStageDto>))]
[JsonSerializable(typeof(List<ProductionProcessStageDto>))]
[JsonSerializable(typeof(ProductionProcessType))]
[JsonSerializable(typeof(ProcessApplicationScope))]
[JsonSerializable(typeof(StageCapacityType))]
[JsonSerializable(typeof(PlannedTimeSource))]

// Production Schedule
[JsonSerializable(typeof(ScheduleListDto))]
[JsonSerializable(typeof(PagedResult<ScheduleListDto>))]
[JsonSerializable(typeof(ScheduleLineDto))]
[JsonSerializable(typeof(IReadOnlyList<ScheduleLineDto>))]
[JsonSerializable(typeof(List<ScheduleLineDto>))]
[JsonSerializable(typeof(ScheduleDetailDto))]
[JsonSerializable(typeof(PendingOrderDto))]
[JsonSerializable(typeof(IReadOnlyList<PendingOrderDto>))]
[JsonSerializable(typeof(List<PendingOrderDto>))]
[JsonSerializable(typeof(ScheduleLineInput))]
[JsonSerializable(typeof(IReadOnlyList<ScheduleLineInput>))]
[JsonSerializable(typeof(List<ScheduleLineInput>))]
[JsonSerializable(typeof(CreateScheduleRequest))]
[JsonSerializable(typeof(UpdateScheduleLinesRequest))]
[JsonSerializable(typeof(ScheduleStatus))]

// Overview Dashboard
[JsonSerializable(typeof(IncompleteOrdersResult))]
[JsonSerializable(typeof(RemainingVolumeItem))]
[JsonSerializable(typeof(IReadOnlyList<RemainingVolumeItem>))]
[JsonSerializable(typeof(List<RemainingVolumeItem>))]
[JsonSerializable(typeof(OutputOverTimeItem))]
[JsonSerializable(typeof(IReadOnlyList<OutputOverTimeItem>))]
[JsonSerializable(typeof(List<OutputOverTimeItem>))]
[JsonSerializable(typeof(OrdersByStatusItem))]
[JsonSerializable(typeof(IReadOnlyList<OrdersByStatusItem>))]
[JsonSerializable(typeof(List<OrdersByStatusItem>))]
[JsonSerializable(typeof(OutputByStageItem))]
[JsonSerializable(typeof(IReadOnlyList<OutputByStageItem>))]
[JsonSerializable(typeof(List<OutputByStageItem>))]
[JsonSerializable(typeof(OutputByDepartmentItem))]
[JsonSerializable(typeof(IReadOnlyList<OutputByDepartmentItem>))]
[JsonSerializable(typeof(List<OutputByDepartmentItem>))]
[JsonSerializable(typeof(TopProductByVolumeItem))]
[JsonSerializable(typeof(IReadOnlyList<TopProductByVolumeItem>))]
[JsonSerializable(typeof(List<TopProductByVolumeItem>))]
[JsonSerializable(typeof(TopProductByErrorRateItem))]
[JsonSerializable(typeof(IReadOnlyList<TopProductByErrorRateItem>))]
[JsonSerializable(typeof(List<TopProductByErrorRateItem>))]
[JsonSerializable(typeof(ErrorRateByCategoryItem))]
[JsonSerializable(typeof(IReadOnlyList<ErrorRateByCategoryItem>))]
[JsonSerializable(typeof(List<ErrorRateByCategoryItem>))]
[JsonSerializable(typeof(StoppageReasonItem))]
[JsonSerializable(typeof(IReadOnlyList<StoppageReasonItem>))]
[JsonSerializable(typeof(List<StoppageReasonItem>))]
[JsonSerializable(typeof(SaveLayoutRequest))]
// document print templates (#98)
[JsonSerializable(typeof(DocumentTemplateDto))]
[JsonSerializable(typeof(IReadOnlyList<DocumentTemplateDto>))]
[JsonSerializable(typeof(List<DocumentTemplateDto>))]
[JsonSerializable(typeof(TemplateFieldItem))]
[JsonSerializable(typeof(IReadOnlyList<TemplateFieldItem>))]
[JsonSerializable(typeof(List<TemplateFieldItem>))]
[JsonSerializable(typeof(PrintAuditLogDto))]
[JsonSerializable(typeof(IReadOnlyList<PrintAuditLogDto>))]
[JsonSerializable(typeof(List<PrintAuditLogDto>))]
[JsonSerializable(typeof(DocumentType))]
[JsonSerializable(typeof(PrintOutputFormat))]
[JsonSerializable(typeof(CreateTemplateRequest))]
[JsonSerializable(typeof(UpdateTemplateRequest))]
// document print available templates (#102)
[JsonSerializable(typeof(AvailableTemplateItem))]
[JsonSerializable(typeof(IReadOnlyList<AvailableTemplateItem>))]
[JsonSerializable(typeof(List<AvailableTemplateItem>))]
// excel import (#105)
[JsonSerializable(typeof(ImportCategoryInfo))]
[JsonSerializable(typeof(IReadOnlyList<ImportCategoryInfo>))]
[JsonSerializable(typeof(List<ImportCategoryInfo>))]
[JsonSerializable(typeof(ImportColumnDef))]
[JsonSerializable(typeof(IReadOnlyList<ImportColumnDef>))]
[JsonSerializable(typeof(List<ImportColumnDef>))]
[JsonSerializable(typeof(ImportRowError))]
[JsonSerializable(typeof(IReadOnlyList<ImportRowError>))]
[JsonSerializable(typeof(List<ImportRowError>))]
[JsonSerializable(typeof(ValidateImportResult))]
[JsonSerializable(typeof(ImportJobSummaryDto))]
[JsonSerializable(typeof(IReadOnlyList<ImportJobSummaryDto>))]
[JsonSerializable(typeof(List<ImportJobSummaryDto>))]
[JsonSerializable(typeof(PagedResult<ImportJobSummaryDto>))]
public partial class ApiJsonContext : JsonSerializerContext
{

}
