using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.DuplicateMachine;

public class DuplicateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow,
    IValidator<DuplicateMachineCommand> validator) : ICommandHandler<DuplicateMachineCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(DuplicateMachineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        var source = await repo.GetByCodeAsync(cmd.SourceCode, ct);
        if (source is null)
            return ValidationResult<string>.NotFound($"Máy nguồn '{cmd.SourceCode}' không tồn tại hoặc đã bị xóa.");

        if (await repo.ExistsAsync(cmd.NewCode, ct))
            return ValidationResult<string>.Invalid(new Dictionary<string, string[]>
            {
                ["NewCode"] = [$"Mã máy '{cmd.NewCode}' đã tồn tại."]
            });

        var copy = Machine.Create(
            cmd.NewCode,
            source.MachineName,
            source.WorkCenterID,
            source.Brand,
            source.Model,
            cmd.CreatedBy);
        copy.UpdateCapacity(
            source.MachineCategory,
            source.TargetOeePct,
            source.TheoreticalCapacityPerHour,
            source.PlannedDowntimeMinPerShift,
            source.HourlyCostRate,
            source.OpcUaNodeId,
            source.RequiresCertification,
            source.CertificationCode,
            source.MaxOperators,
            cmd.CreatedBy ?? "system");

        await repo.AddAsync(copy, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<string>.Ok(copy.MachineCode);
    }
}
