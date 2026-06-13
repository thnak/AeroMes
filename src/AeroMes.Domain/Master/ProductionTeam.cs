using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

/// <summary>
/// Production team (Tổ sản xuất) — the operational unit executing manufacturing
/// processes. Carries productivity standards used by order-based planning.
/// </summary>
public class ProductionTeam : AuditableEntity
{
    public string TeamCode { get; private set; } = string.Empty;
    public string TeamName { get; private set; } = string.Empty;
    public int? OrgUnitId { get; private set; }
    public int? StandardLaborQuantity { get; private set; }
    public decimal? ProductionRate { get; private set; } // output units per worker-hour
    public bool IsOrderBasedPlanningEnabled { get; private set; }
    public bool IsActive { get; private set; } = true;

    public OrgUnit? OrgUnit { get; private set; }

    private readonly List<ProductionTeamMember> _members = [];
    public IReadOnlyList<ProductionTeamMember> Members => _members.AsReadOnly();

    private readonly List<ProductionTeamProductGroup> _productGroups = [];
    public IReadOnlyList<ProductionTeamProductGroup> ProductGroups => _productGroups.AsReadOnly();

    private ProductionTeam() { }

    public static ProductionTeam Create(
        string code, string name, int? orgUnitId,
        int? standardLaborQuantity, decimal? productionRate,
        bool isOrderBasedPlanningEnabled,
        IEnumerable<int> productGroupCategoryIds,
        string? createdBy)
    {
        var team = new ProductionTeam
        {
            TeamCode = code.Trim().ToUpperInvariant(),
            TeamName = name.Trim(),
            OrgUnitId = orgUnitId,
            StandardLaborQuantity = standardLaborQuantity,
            ProductionRate = productionRate,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        team.SetProductGroups(productGroupCategoryIds, updatedBy: null);
        team.SetOrderBasedPlanning(isOrderBasedPlanningEnabled, updatedBy: null);
        return team;
    }

    public void UpdateDetails(
        string name, int? orgUnitId,
        int? standardLaborQuantity, decimal? productionRate,
        bool isOrderBasedPlanningEnabled, bool isActive,
        IEnumerable<int> productGroupCategoryIds,
        string? updatedBy)
    {
        TeamName = name.Trim();
        OrgUnitId = orgUnitId;
        StandardLaborQuantity = standardLaborQuantity;
        ProductionRate = productionRate;
        IsActive = isActive;
        SetProductGroups(productGroupCategoryIds, updatedBy);
        SetOrderBasedPlanning(isOrderBasedPlanningEnabled, updatedBy);
        Touch(updatedBy);
    }

    private void SetOrderBasedPlanning(bool enabled, string? updatedBy)
    {
        if (enabled && _productGroups.Count == 0)
            throw new DomainException(
                "Phải chọn ít nhất một nhóm sản phẩm trước khi bật lập kế hoạch theo lệnh sản xuất.");
        IsOrderBasedPlanningEnabled = enabled;
        Touch(updatedBy);
    }

    private void SetProductGroups(IEnumerable<int> categoryIds, string? updatedBy)
    {
        var target = categoryIds.Distinct().ToHashSet();
        _productGroups.RemoveAll(g => !target.Contains(g.CategoryId));
        foreach (var id in target.Where(id => _productGroups.All(g => g.CategoryId != id)))
            _productGroups.Add(ProductionTeamProductGroup.Create(TeamCode, id));
        Touch(updatedBy);
    }

    // ── Worker roster ────────────────────────────────────────────────────────

    public ProductionTeamMember AddMember(string employeeCode, bool isLeader, string? updatedBy)
    {
        var normalized = employeeCode.Trim().ToUpperInvariant();
        if (_members.Any(m => m.EmployeeCode == normalized))
            throw new DomainException($"Nhân viên '{normalized}' đã có trong tổ '{TeamCode}'.");

        var member = ProductionTeamMember.Create(TeamCode, normalized, isLeader);
        _members.Add(member);
        Touch(updatedBy);
        return member;
    }

    public void RemoveMember(string employeeCode, string? updatedBy)
    {
        var normalized = employeeCode.Trim().ToUpperInvariant();
        var member = _members.FirstOrDefault(m => m.EmployeeCode == normalized)
            ?? throw new DomainException($"ProductionTeamMember '{normalized}' was not found.");
        _members.Remove(member);
        Touch(updatedBy);
    }

    /// <summary>Copies every configuration field, roster, and product group — only the code differs.</summary>
    public ProductionTeam Duplicate(string newCode, string? createdBy)
    {
        var copy = Create(
            newCode, TeamName, OrgUnitId,
            StandardLaborQuantity, ProductionRate,
            IsOrderBasedPlanningEnabled,
            _productGroups.Select(g => g.CategoryId),
            createdBy);
        foreach (var member in _members)
            copy.AddMember(member.EmployeeCode, member.IsLeader, createdBy);
        return copy;
    }
}
