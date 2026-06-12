using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.CreateEmployee;

public class CreateEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateEmployeeCommand, string>
{
    public async Task<string> HandleAsync(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = Employee.Create(
            cmd.Code, cmd.FullName, cmd.Department,
            cmd.RoleType, cmd.DefaultWorkCenterId,
            cmd.CreatedBy);
        await repo.AddAsync(employee, ct);
        await uow.SaveChangesAsync(ct);
        return employee.EmployeeCode;
    }
}
