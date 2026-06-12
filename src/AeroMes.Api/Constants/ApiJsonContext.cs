using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Api.Middleware;
using AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;
using AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Application.Jobs.Queries.GetJobDetail;
using AeroMes.Application.Jobs.Queries.GetJobs;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;
using AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;
using AeroMes.Application.Master.DowntimeReasonCodes.Queries.GetDowntimeReasonCodes;
using AeroMes.Application.Master.Products.Queries.GetProducts;
using AeroMes.Application.Master.ShiftTemplates.Queries.GetShiftTemplates;
using AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;
using AeroMes.Application.Master.Suppliers.Queries.GetSupplierById;
using AeroMes.Application.Master.Suppliers.Queries.GetSuppliers;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Constants;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ProblemDetails))]
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
// capability groups
[JsonSerializable(typeof(IReadOnlyList<CapabilityGroupDto>))]
[JsonSerializable(typeof(CapabilityGroupCreatedResult))]
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
// products
[JsonSerializable(typeof(IReadOnlyList<ProductDto>))]
[JsonSerializable(typeof(ProductDetailDto))]
[JsonSerializable(typeof(ProductCreatedResult))]
[JsonSerializable(typeof(CreateProductRequest))]
[JsonSerializable(typeof(UpdateProductRequest))]
[JsonSerializable(typeof(ChangeLifecycleRequest))]
// downtime reason codes
[JsonSerializable(typeof(IReadOnlyList<DowntimeReasonCodeDto>))]
[JsonSerializable(typeof(DowntimeReasonCodeCreatedResult))]
[JsonSerializable(typeof(CreateDowntimeReasonCodeRequest))]
[JsonSerializable(typeof(UpdateDowntimeReasonCodeRequest))]
// alert thresholds
[JsonSerializable(typeof(IReadOnlyList<AlertThresholdDto>))]
[JsonSerializable(typeof(AlertThresholdCreatedResult))]
[JsonSerializable(typeof(CreateAlertThresholdRequest))]
[JsonSerializable(typeof(UpdateAlertThresholdRequest))]
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
public partial class ApiJsonContext : JsonSerializerContext
{

}
