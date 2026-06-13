using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class ProductFamily
{
    public string FamilyCode { get; private set; } = string.Empty;
    public string FamilyName { get; private set; } = string.Empty;
    public string BaseProductCode { get; private set; } = string.Empty;
    public string Industry { get; private set; } = FamilyIndustries.General;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private readonly List<VariantDimension> _dimensions = [];
    public IReadOnlyList<VariantDimension> Dimensions => _dimensions.AsReadOnly();

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();

    private ProductFamily() { }

    public static ProductFamily Create(
        string familyCode,
        string familyName,
        string baseProductCode,
        string industry)
    {
        if (!FamilyIndustries.All.Contains(industry))
            throw new DomainException($"Industry '{industry}' is not valid.");

        return new ProductFamily
        {
            FamilyCode = familyCode.Trim().ToUpperInvariant(),
            FamilyName = familyName.Trim(),
            BaseProductCode = baseProductCode.Trim().ToUpperInvariant(),
            Industry = industry,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public VariantDimension AddDimension(string dimensionName, int sortOrder, bool isRequired = true)
    {
        var normalized = dimensionName.Trim();
        if (_dimensions.Any(d => d.DimensionName.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Dimension '{normalized}' already exists in family '{FamilyCode}'.");

        var dim = VariantDimension.Create(FamilyCode, normalized, sortOrder, isRequired);
        _dimensions.Add(dim);
        return dim;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public class VariantDimension
{
    public int DimensionID { get; private set; }
    public string FamilyCode { get; private set; } = string.Empty;
    public string DimensionName { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; } = true;

    private readonly List<VariantDimensionValue> _values = [];
    public IReadOnlyList<VariantDimensionValue> Values => _values.AsReadOnly();

    private VariantDimension() { }

    internal static VariantDimension Create(string familyCode, string dimensionName, int sortOrder, bool isRequired)
        => new() { FamilyCode = familyCode, DimensionName = dimensionName, SortOrder = sortOrder, IsRequired = isRequired };

    public VariantDimensionValue AddValue(string valueCode, string valueLabel, int sortOrder)
    {
        var normalized = valueCode.Trim().ToUpperInvariant();
        if (_values.Any(v => v.ValueCode == normalized))
            throw new DomainException($"Value '{normalized}' already exists in dimension '{DimensionName}'.");

        var val = VariantDimensionValue.Create(DimensionID, normalized, valueLabel.Trim(), sortOrder);
        _values.Add(val);
        return val;
    }
}

public class VariantDimensionValue
{
    public int ValueID { get; private set; }
    public int DimensionID { get; private set; }
    public string ValueCode { get; private set; } = string.Empty;
    public string ValueLabel { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private VariantDimensionValue() { }

    internal static VariantDimensionValue Create(int dimensionId, string code, string label, int sortOrder)
        => new() { DimensionID = dimensionId, ValueCode = code, ValueLabel = label, SortOrder = sortOrder, IsActive = true };
}

public class ProductVariant
{
    public int VariantID { get; private set; }
    public string FamilyCode { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string VariantKey { get; private set; } = string.Empty;
    public string VariantAttributes { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;

    private ProductVariant() { }

    public static ProductVariant Create(string familyCode, string productCode, string variantKey, string variantAttributes)
        => new()
        {
            FamilyCode = familyCode,
            ProductCode = productCode,
            VariantKey = variantKey,
            VariantAttributes = variantAttributes,
            IsActive = true,
        };
}

public static class FamilyIndustries
{
    public const string General     = "GENERAL";
    public const string Plastic     = "PLASTIC";
    public const string Apparel     = "APPAREL";
    public const string Dairy       = "DAIRY";
    public const string Electronics = "ELECTRONICS";

    public static readonly IReadOnlyList<string> All =
        [General, Plastic, Apparel, Dairy, Electronics];
}
