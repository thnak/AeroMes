using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Api.Middleware;
using AeroMes.Application.Integration.Queries.GetProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetProductionOrders;
using AeroMes.Application.Integration.Queries.GetSalesOrderDetail;
using AeroMes.Application.Integration.Queries.GetSalesOrders;
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
using AeroMes.Application.Master.Customers.Queries.GetCustomerById;
using AeroMes.Application.Master.Customers.Queries.GetCustomers;
using AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using AeroMes.Application.Master.Employees.Queries.GetEmployees;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;
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
// integration
[JsonSerializable(typeof(IReadOnlyList<SalesOrderDto>))]
[JsonSerializable(typeof(SalesOrderDetailDto))]
[JsonSerializable(typeof(IReadOnlyList<ProductionOrderSummaryDto>))]
[JsonSerializable(typeof(IReadOnlyList<ProductionOrderDto>))]
[JsonSerializable(typeof(ProductionOrderDetailDto))]
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
public partial class ApiJsonContext : JsonSerializerContext
{

}
