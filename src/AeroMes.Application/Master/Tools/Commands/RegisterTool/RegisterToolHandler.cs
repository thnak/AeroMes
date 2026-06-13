using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RegisterTool;
public class RegisterToolHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<RegisterToolCommand> validator) : ICommandHandler<RegisterToolCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(RegisterToolCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());
        try
        {
            var tool = Tool.Create(
                cmd.Code, cmd.Name, cmd.ToolType,
                cmd.Brand, cmd.Model, cmd.Specification,
                cmd.MaxUsageCount, cmd.PmIntervalCount,
                cmd.RequiresCalibration, cmd.CalibrationIntervalDays,
                cmd.StorageLocation, cmd.PurchaseDate, cmd.PurchaseCost,
                cmd.Notes, cmd.CreatedBy);
            await repo.AddAsync(tool, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(tool.ToolCode);
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
