using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using AeroMes.Application.Master.Employees.Queries.GetEmployees;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class EmployeeTests(AeroMesWebFactory factory)
{
    // ── CRUD ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/employees");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201WithCode()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("EMP");

        var response = await client.PostAsJsonAsync("/api/v1/employees",
            new { Code = code, FullName = "Nguyen Van A", Department = "Pressing", RoleType = "Operator" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.Equal(code, body?.EmployeeCode);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "First", RoleType = "Operator" });

        var response = await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "Duplicate", RoleType = "Operator" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingEmployee_Returns204AndPersistsChange()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("UPD");

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "Original", RoleType = "Operator" });

        var updateResp = await client.PutAsJsonAsync($"/api/v1/employees/{code}",
            new { FullName = "Updated", Department = "QC Lab", RoleType = "Qc", IsActive = true });

        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/employees/{code}"))
            .Content.ReadFromJsonAsync<EmployeeDetailDto>();
        Assert.Equal("Updated", detail!.FullName);
        Assert.Equal("Qc", detail.RoleType);
    }

    [Fact]
    public async Task Delete_ExistingEmployee_Returns204AndHidesFromList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEL");

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "To Delete", RoleType = "Operator" });

        var deleteResp = await client.DeleteAsync($"/api/v1/employees/{code}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/employees?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<EmployeeDto>>();
        Assert.DoesNotContain(items!, x => x.EmployeeCode == code);
    }

    // ── Skills ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetSkill_TwiceForSameOperation_UpsertsSingleSkill()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("SKL");
        var operationCode = await SeedOperationAsync();

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "Skilled Op", RoleType = "Operator" });

        var first = await client.PutAsJsonAsync($"/api/v1/employees/{code}/skills",
            new { OperationCode = operationCode, CertificationLevel = 2, CertifiedAt = "2026-01-15" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PutAsJsonAsync($"/api/v1/employees/{code}/skills",
            new { OperationCode = operationCode, CertificationLevel = 4, CertifiedAt = "2026-06-01" });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/employees/{code}"))
            .Content.ReadFromJsonAsync<EmployeeDetailDto>();
        var skill = Assert.Single(detail!.Skills);
        Assert.Equal(4, skill.CertificationLevel);
    }

    [Fact]
    public async Task SetSkill_InvalidLevel_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("SKX");
        var operationCode = await SeedOperationAsync();

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "Bad Level", RoleType = "Operator" });

        var response = await client.PutAsJsonAsync($"/api/v1/employees/{code}/skills",
            new { OperationCode = operationCode, CertificationLevel = 9, CertifiedAt = "2026-01-15" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Shift assignments & schedule ──────────────────────────────────────────

    [Fact]
    public async Task Schedule_ReturnsOnlyAssignmentsEffectiveOnDate()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("SCH");
        var (workCenterId, shiftCode) = await SeedWorkCenterAndShiftAsync();

        await client.PostAsJsonAsync("/api/v1/employees", new { Code = code, FullName = "Scheduled Op", RoleType = "Operator" });

        var addResp = await client.PostAsJsonAsync($"/api/v1/employees/{code}/shift-assignments",
            new { WorkCenterId = workCenterId, ShiftCode = shiftCode, ValidFrom = "2026-06-01", ValidTo = "2026-06-30" });
        Assert.Equal(HttpStatusCode.Created, addResp.StatusCode);

        var inRange = await (await client.GetAsync($"/api/v1/employees/{code}/schedule?asOf=2026-06-15"))
            .Content.ReadFromJsonAsync<EmployeeScheduleDto>();
        var assignment = Assert.Single(inRange!.Assignments);
        Assert.Equal(shiftCode, assignment.ShiftCode);
        Assert.NotNull(assignment.StartTime);

        var outOfRange = await (await client.GetAsync($"/api/v1/employees/{code}/schedule?asOf=2026-07-15"))
            .Content.ReadFromJsonAsync<EmployeeScheduleDto>();
        Assert.Empty(outOfRange!.Assignments);
    }

    // ── StartJob certification gate ───────────────────────────────────────────

    [Fact]
    public async Task StartJob_UnknownOperator_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var seed = await SeedRunningWorkOrderAsync(requireCertification: false);

        var response = await client.PostAsJsonAsync("/api/v1/jobs",
            new { WorkOrderId = seed.WorkOrderId, MachineCode = seed.MachineCode, ShiftCode = seed.ShiftCode, OperatorId = "GHOST-OP" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task StartJob_CertificationRequired_BlocksUncertifiedThenAllowsCertified()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var seed = await SeedRunningWorkOrderAsync(requireCertification: true);
        var operatorCode = UniqueCode("OPR");

        await client.PostAsJsonAsync("/api/v1/employees",
            new { Code = operatorCode, FullName = "Uncertified Op", RoleType = "Operator" });

        var blocked = await client.PostAsJsonAsync("/api/v1/jobs",
            new { WorkOrderId = seed.WorkOrderId, MachineCode = seed.MachineCode, ShiftCode = seed.ShiftCode, OperatorId = operatorCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        await client.PutAsJsonAsync($"/api/v1/employees/{operatorCode}/skills",
            new { OperationCode = seed.OperationCode, CertificationLevel = 3, CertifiedAt = "2026-01-01" });

        var allowed = await client.PostAsJsonAsync("/api/v1/jobs",
            new { WorkOrderId = seed.WorkOrderId, MachineCode = seed.MachineCode, ShiftCode = seed.ShiftCode, OperatorId = operatorCode });
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
    }

    [Fact]
    public async Task StartJob_CertificationNotRequired_AllowsAnyActiveEmployee()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var seed = await SeedRunningWorkOrderAsync(requireCertification: false);
        var operatorCode = UniqueCode("OPN");

        await client.PostAsJsonAsync("/api/v1/employees",
            new { Code = operatorCode, FullName = "Plain Op", RoleType = "Operator" });

        var response = await client.PostAsJsonAsync("/api/v1/jobs",
            new { WorkOrderId = seed.WorkOrderId, MachineCode = seed.MachineCode, ShiftCode = seed.ShiftCode, OperatorId = operatorCode });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private async Task<string> SeedOperationAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var operation = Operation.Create(UniqueCode("OP"), "Test Operation");
        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        return operation.OperationCode;
    }

    private async Task<(int WorkCenterId, string ShiftCode)> SeedWorkCenterAndShiftAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workCenter = WorkCenter.Create(UniqueCode("WC"), "Test WC");
        db.WorkCenters.Add(workCenter);
        var shift = ShiftTemplate.Create(UniqueCode("SH")[..8], "Day Shift", new TimeOnly(6, 0), new TimeOnly(14, 0));
        db.ShiftTemplates.Add(shift);
        await db.SaveChangesAsync();
        return (workCenter.WorkCenterID, shift.ShiftCode);
    }

    private sealed record WorkOrderSeed(int WorkOrderId, string MachineCode, string ShiftCode, string OperationCode);

    private async Task<WorkOrderSeed> SeedRunningWorkOrderAsync(bool requireCertification)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var uom = UnitOfMeasure.Create($"U{suffix[..5]}", "Each", "QTY");
        db.UnitsOfMeasure.Add(uom);
        var workCenter = WorkCenter.Create($"WC-{suffix}", "Cert WC");
        db.WorkCenters.Add(workCenter);
        var operation = Operation.Create($"OP-{suffix}", "Cert Operation");
        db.Operations.Add(operation);
        await db.SaveChangesAsync();

        var product = Product.Create($"P-{suffix}", "Cert Product", uom.UoMCode,
            ItemType.FG, null, null, false, false, null, null, null, null, null, null);
        db.Products.Add(product);
        var routing = Routing.Create($"R-{suffix}", "Cert Routing", $"P-{suffix}");
        db.Routings.Add(routing);
        await db.SaveChangesAsync();

        var step = RoutingStep.Create(routing.RoutingID, 1, operation.OperationCode, workCenter.WorkCenterID);
        db.RoutingSteps.Add(step);
        var productionOrder = ProductionOrder.CreateFromErp($"PO-{suffix}", $"P-{suffix}", 100);
        db.ProductionOrders.Add(productionOrder);
        await db.SaveChangesAsync();

        var workOrder = WorkOrder.Create($"WO-{suffix}", productionOrder.POID, step.RoutingStepID, workCenter.WorkCenterID, 100);
        workOrder.Start("seed");
        db.WorkOrders.Add(workOrder);
        var machine = Machine.Create($"M-{suffix}", "Cert Machine", workCenter.WorkCenterID);
        db.Machines.Add(machine);
        var shift = ShiftTemplate.Create($"S{suffix[..6]}", "Cert Shift", new TimeOnly(6, 0), new TimeOnly(14, 0));
        db.ShiftTemplates.Add(shift);
        // Scoped to this test's work center so other tests in the collection are unaffected.
        var rules = Domain.Master.WorkOrderAutoRules.Create(
            workCenter.WorkCenterID, false, false, false, 1, requireCertification);
        db.WorkOrderAutoRules.Add(rules);
        await db.SaveChangesAsync();

        return new WorkOrderSeed(workOrder.WOID, machine.MachineCode, shift.ShiftCode, operation.OperationCode);
    }

    private record CreatedResult(string EmployeeCode);
}
