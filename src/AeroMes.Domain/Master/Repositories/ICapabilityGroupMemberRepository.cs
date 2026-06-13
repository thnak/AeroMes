namespace AeroMes.Domain.Master.Repositories;

public interface ICapabilityGroupMemberRepository
{
    Task<IReadOnlyList<CapabilityGroupMember>> GetByGroupAsync(string groupCode, CancellationToken ct = default);
    Task<CapabilityGroupMember?> GetByIdAsync(int memberId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string groupCode, CapabilityResourceType resourceType, string resourceId, CancellationToken ct = default);
    Task<bool> GroupHasMembersAsync(string groupCode, CancellationToken ct = default);
    Task AddAsync(CapabilityGroupMember entity, CancellationToken ct = default);
    void Remove(CapabilityGroupMember entity);
}
