using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Employees.Commands.AddShiftAssignment;

public class AddShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow,
    IValidator<AddShiftAssignmentCommand> validator) : ICommandHandler<AddShiftAssignmentCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct);
            if (employee is null) return ValidationResult<int>.NotFound($"Entity '{cmd.EmployeeCode}' was not found.");
            var assignment = employee.AddShiftAssignment(
                cmd.WorkCenterId, cmd.ShiftCode,
                cmd.ValidFrom, cmd.ValidTo);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(assignment.ShiftAssignmentId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
