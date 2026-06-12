using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CustomerPartNumber : Entity
{
    public int CustomerPartNumberId { get; private set; }
    public string CustomerCode { get; private set; } = string.Empty;
    public string CustomerPartNo { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? DrawingReference { get; private set; }
    public string? Revision { get; private set; }

    public Product? Product { get; private set; }

    private CustomerPartNumber() { }

    internal static CustomerPartNumber Create(
        string customerCode, string customerPartNo, string productCode,
        string? description, string? drawingReference, string? revision)
    {
        return new CustomerPartNumber
        {
            CustomerCode = customerCode,
            CustomerPartNo = customerPartNo.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            DrawingReference = drawingReference?.Trim(),
            Revision = revision?.Trim(),
        };
    }

    internal void Update(string? description, string? drawingReference, string? revision)
    {
        Description = description?.Trim();
        DrawingReference = drawingReference?.Trim();
        Revision = revision?.Trim();
    }
}
