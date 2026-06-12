using AeroMes.Api.Auth;
using AeroMes.Application.Master.Employees.Commands.AddShiftAssignment;
using AeroMes.Application.Master.Employees.Commands.CreateEmployee;
using AeroMes.Application.Master.Employees.Commands.DeleteEmployee;
using AeroMes.Application.Master.Employees.Commands.EndShiftAssignment;
using AeroMes.Application.Master.Employees.Commands.RemoveEmployeeSkill;
using AeroMes.Application.Master.Employees.Commands.RemoveShiftAssignment;
using AeroMes.Application.Master.Employees.Commands.SetEmployeeSkill;
using AeroMes.Application.Master.Employees.Commands.UpdateEmployee;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using AeroMes.Application.Master.Employees.Queries.GetEmployees;
using AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<EmployeeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetEmployeesQuery(activeOnly), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<EmployeeDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetEmployeeByIdQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{code}/schedule")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<EmployeeScheduleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(string code, [FromQuery] DateOnly? asOf = null, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new GetEmployeeScheduleQuery(code, asOf), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<EmployeeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateEmployeeCommand(
                req.Code, req.FullName, req.Department,
                req.RoleType, req.DefaultWorkCenterId,
                User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetById), new { code }, new EmployeeCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateEmployeeRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateEmployeeCommand(
                code, req.FullName, req.Department,
                req.RoleType, req.DefaultWorkCenterId,
                req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteEmployeeCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── Skills sub-resource ─────────────────────────────────────────────────

    [HttpPut("{code}/skills")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<EmployeeSkillSavedResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetSkill(string code, [FromBody] SetEmployeeSkillRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new SetEmployeeSkillCommand(
                code, req.OperationCode, req.CertificationLevel,
                req.CertifiedAt, req.ExpiresAt,
                User.Identity?.Name), null, ct);
        return Ok(new EmployeeSkillSavedResult(id));
    }

    [HttpDelete("{code}/skills/{skillId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSkill(string code, int skillId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveEmployeeSkillCommand(code, skillId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── Shift assignments sub-resource ──────────────────────────────────────

    [HttpPost("{code}/shift-assignments")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ShiftAssignmentCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddShiftAssignment(string code, [FromBody] AddShiftAssignmentRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new AddShiftAssignmentCommand(
                code, req.WorkCenterId, req.ShiftCode,
                req.ValidFrom, req.ValidTo,
                User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetById), new { code }, new ShiftAssignmentCreatedResult(id));
    }

    [HttpPut("{code}/shift-assignments/{assignmentId:int}/end")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EndShiftAssignment(string code, int assignmentId, [FromBody] EndShiftAssignmentRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new EndShiftAssignmentCommand(code, assignmentId, req.ValidTo, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}/shift-assignments/{assignmentId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveShiftAssignment(string code, int assignmentId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveShiftAssignmentCommand(code, assignmentId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateEmployeeRequest(
    string Code,
    string FullName,
    string? Department,
    EmployeeRoleType RoleType,
    int? DefaultWorkCenterId);

public record UpdateEmployeeRequest(
    string FullName,
    string? Department,
    EmployeeRoleType RoleType,
    int? DefaultWorkCenterId,
    bool IsActive);

public record SetEmployeeSkillRequest(
    string OperationCode,
    int CertificationLevel,
    DateOnly CertifiedAt,
    DateOnly? ExpiresAt);

public record AddShiftAssignmentRequest(
    int WorkCenterId,
    string ShiftCode,
    DateOnly ValidFrom,
    DateOnly? ValidTo);

public record EndShiftAssignmentRequest(DateOnly ValidTo);

public record EmployeeCreatedResult(string EmployeeCode);
public record EmployeeSkillSavedResult(int EmployeeSkillId);
public record ShiftAssignmentCreatedResult(int ShiftAssignmentId);
