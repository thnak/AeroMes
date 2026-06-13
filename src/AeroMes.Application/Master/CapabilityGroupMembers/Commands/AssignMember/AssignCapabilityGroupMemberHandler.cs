using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroupMembers.Commands.AssignMember;

public class AssignCapabilityGroupMemberHandler(
    ICapabilityGroupRepository groupRepo,
    ICapabilityGroupMemberRepository memberRepo,
    IMachineRepository machineRepo,
    IProductionTeamRepository teamRepo,
    IToolRepository toolRepo,
    IMoldRepository moldRepo,
    IUnitOfWork uow,
    IValidator<AssignCapabilityGroupMemberCommand> validator)
    : ICommandHandler<AssignCapabilityGroupMemberCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AssignCapabilityGroupMemberCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var group = await groupRepo.GetByCodeAsync(cmd.GroupCode, ct);
        if (group is null)
            return ValidationResult<int>.NotFound($"Nhóm năng lực '{cmd.GroupCode}' không tồn tại.");

        if (!group.IsActive)
            return ValidationResult<int>.Failure($"Nhóm năng lực '{cmd.GroupCode}' đã bị vô hiệu hóa.");

        var resourceExists = cmd.ResourceType switch
        {
            CapabilityResourceType.Machine => await machineRepo.ExistsAsync(cmd.ResourceId, ct),
            CapabilityResourceType.ProductionTeam => await teamRepo.CodeExistsAsync(cmd.ResourceId, ct),
            CapabilityResourceType.Tool => await toolRepo.CodeExistsAsync(cmd.ResourceId, ct),
            CapabilityResourceType.Mold => await moldRepo.CodeExistsAsync(cmd.ResourceId, ct),
            _ => false
        };

        if (!resourceExists)
            return ValidationResult<int>.NotFound($"Tài nguyên '{cmd.ResourceId}' ({cmd.ResourceType}) không tồn tại hoặc đã bị xóa.");

        if (await memberRepo.ExistsAsync(cmd.GroupCode, cmd.ResourceType, cmd.ResourceId, ct))
            return ValidationResult<int>.Failure($"Tài nguyên '{cmd.ResourceId}' đã thuộc nhóm '{cmd.GroupCode}'.");

        var member = CapabilityGroupMember.Create(cmd.GroupCode, cmd.ResourceType, cmd.ResourceId, cmd.CreatedBy);
        await memberRepo.AddAsync(member, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(member.MemberId);
    }
}
