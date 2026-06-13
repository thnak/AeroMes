using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CapabilityGroupMemberRepository(AppDbContext db) : ICapabilityGroupMemberRepository
{
    public async Task<IReadOnlyList<CapabilityGroupMember>> GetByGroupAsync(string groupCode, CancellationToken ct) =>
        await db.CapabilityGroupMembers
            .Where(x => x.GroupCode == groupCode.ToUpperInvariant())
            .OrderBy(x => x.ResourceType).ThenBy(x => x.ResourceId)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<CapabilityGroupMember?> GetByIdAsync(int memberId, CancellationToken ct) =>
        db.CapabilityGroupMembers.FirstOrDefaultAsync(x => x.MemberId == memberId, ct);

    public Task<bool> ExistsAsync(string groupCode, CapabilityResourceType resourceType, string resourceId, CancellationToken ct) =>
        db.CapabilityGroupMembers.AnyAsync(
            x => x.GroupCode == groupCode.ToUpperInvariant()
              && x.ResourceType == resourceType
              && x.ResourceId == resourceId.ToUpperInvariant(),
            ct);

    public Task<bool> GroupHasMembersAsync(string groupCode, CancellationToken ct) =>
        db.CapabilityGroupMembers.AnyAsync(x => x.GroupCode == groupCode.ToUpperInvariant(), ct);

    public Task AddAsync(CapabilityGroupMember entity, CancellationToken ct)
    {
        db.CapabilityGroupMembers.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(CapabilityGroupMember entity) =>
        db.CapabilityGroupMembers.Remove(entity);
}
