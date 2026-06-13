using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateBin;

public class CreateBinHandler(
    IBinRepository repo,
    IRackRepository rackRepo,
    IUnitOfWork uow,
    IValidator<CreateBinCommand> validator)
    : ICommandHandler<CreateBinCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateBinCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var rack = await rackRepo.GetByIdAsync(cmd.RackId, ct);
        if (rack is null)
            return ValidationResult<int>.NotFound($"Kệ '{cmd.RackId}' không tồn tại.");

        if (await repo.CodeExistsInRackAsync(cmd.RackId, cmd.BinCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["BinCode"] = [$"Mã ô '{cmd.BinCode}' đã tồn tại trong kệ này."]
            });

        var entity = Bin.Create(cmd.RackId, cmd.BinCode, cmd.BinLevel, cmd.BinType, cmd.MaxQty, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(entity.BinId);
    }
}
