using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Commands.UnassignMember;

public class UnassignCapabilityGroupMemberHandler(
    ICapabilityGroupMemberRepository memberRepo,
    IUnitOfWork uow)
    : ICommandHandler<UnassignCapabilityGroupMemberCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UnassignCapabilityGroupMemberCommand cmd, CancellationToken ct)
    {
        var member = await memberRepo.GetByIdAsync(cmd.MemberId, ct);
        if (member is null)
            return ValidationResult<Unit>.NotFound($"Thành viên {cmd.MemberId} không tồn tại.");

        if (!string.Equals(member.GroupCode, cmd.GroupCode, StringComparison.OrdinalIgnoreCase))
            return ValidationResult<Unit>.NotFound($"Thành viên {cmd.MemberId} không thuộc nhóm '{cmd.GroupCode}'.");

        memberRepo.Remove(member);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
