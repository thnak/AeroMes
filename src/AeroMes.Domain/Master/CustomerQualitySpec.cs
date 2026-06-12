using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CustomerQualitySpec : Entity
{
    public int CustomerQualitySpecId { get; private set; }
    public string CustomerCode { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string? AqlLevel { get; private set; }
    public InspectionLevel? InspectionLevel { get; private set; }
    public string? AcceptanceCriteria { get; private set; }
    public int? MaxDefectsPpm { get; private set; }
    public string? SpecialRequirements { get; private set; }
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    public Product? Product { get; private set; }

    private CustomerQualitySpec() { }

    internal static CustomerQualitySpec Create(
        string customerCode, string productCode,
        string? aqlLevel, InspectionLevel? inspectionLevel,
        string? acceptanceCriteria, int? maxDefectsPpm,
        string? specialRequirements,
        DateOnly? effectiveFrom, DateOnly? effectiveTo)
    {
        return new CustomerQualitySpec
        {
            CustomerCode = customerCode,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            AqlLevel = aqlLevel?.Trim(),
            InspectionLevel = inspectionLevel,
            AcceptanceCriteria = acceptanceCriteria?.Trim(),
            MaxDefectsPpm = maxDefectsPpm,
            SpecialRequirements = specialRequirements?.Trim(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
        };
    }

    internal void Update(
        string? aqlLevel, InspectionLevel? inspectionLevel,
        string? acceptanceCriteria, int? maxDefectsPpm,
        string? specialRequirements,
        DateOnly? effectiveFrom, DateOnly? effectiveTo)
    {
        AqlLevel = aqlLevel?.Trim();
        InspectionLevel = inspectionLevel;
        AcceptanceCriteria = acceptanceCriteria?.Trim();
        MaxDefectsPpm = maxDefectsPpm;
        SpecialRequirements = specialRequirements?.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
}
