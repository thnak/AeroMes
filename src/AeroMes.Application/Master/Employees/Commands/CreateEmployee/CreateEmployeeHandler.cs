using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.CreateEmployee;
public class CreateEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateEmployeeCommand> validator) : ICommandHandler<CreateEmployeeCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());
        try
        {
            var employee = Employee.Create(
                cmd.Code, cmd.FullName, cmd.Department,
                cmd.RoleType, cmd.DefaultWorkCenterId,
                cmd.CreatedBy);
            await repo.AddAsync(employee, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(employee.EmployeeCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
