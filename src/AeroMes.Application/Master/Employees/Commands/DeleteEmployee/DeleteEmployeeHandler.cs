using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteEmployeeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdAsync(cmd.Code, ct);
        if (employee is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.Code}' was not found.");
        employee.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
