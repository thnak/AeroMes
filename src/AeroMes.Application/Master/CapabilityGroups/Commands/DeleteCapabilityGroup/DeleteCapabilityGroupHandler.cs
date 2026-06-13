using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.DeleteCapabilityGroup;

public class DeleteCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    ICapabilityGroupMemberRepository memberRepo,
    IUnitOfWork uow) : ICommandHandler<DeleteCapabilityGroupCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Nhóm năng lực '{cmd.Code}' không tồn tại.");

        if (await memberRepo.GroupHasMembersAsync(cmd.Code, ct))
            return ValidationResult<Unit>.Failure($"Nhóm '{cmd.Code}' còn thành viên — hãy hủy gán thành viên hoặc vô hiệu hóa nhóm thay vì xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
