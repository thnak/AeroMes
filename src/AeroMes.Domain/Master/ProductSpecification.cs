namespace AeroMes.Domain.Master;

public class ProductSpecification
{
    public int SpecificationId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string SpecCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ProductSpecification() { }

    internal static ProductSpecification Create(string productCode, string specCode, string? description)
    {
        return new ProductSpecification
        {
            ProductCode = productCode,
            SpecCode = specCode.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            IsActive = true,
        };
    }

    internal void Update(string? description, bool isActive)
    {
        Description = description?.Trim();
        IsActive = isActive;
    }
}
