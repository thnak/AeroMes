using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

/// <summary>Product group (category) a production team is qualified to manufacture.</summary>
public class ProductionTeamProductGroup : Entity
{
    public int LinkId { get; private set; }
    public string TeamCode { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }

    public ProductCategory? Category { get; private set; }

    private ProductionTeamProductGroup() { }

    internal static ProductionTeamProductGroup Create(string teamCode, int categoryId)
    {
        return new ProductionTeamProductGroup
        {
            TeamCode = teamCode,
            CategoryId = categoryId,
        };
    }
}
