using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Customer : AuditableEntity
{
    public string CustomerCode { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public CustomerType CustomerType { get; private set; } = CustomerType.Direct;
    public string? TaxId { get; private set; }
    public string? Country { get; private set; }
    public string? Address { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ContactEmail { get; private set; }
    public int CreditTermsDays { get; private set; } = 30;
    public string Currency { get; private set; } = "VND";
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private readonly List<CustomerPartNumber> _partNumbers = [];
    public IReadOnlyList<CustomerPartNumber> PartNumbers => _partNumbers.AsReadOnly();

    private readonly List<CustomerQualitySpec> _qualitySpecs = [];
    public IReadOnlyList<CustomerQualitySpec> QualitySpecs => _qualitySpecs.AsReadOnly();

    private Customer() { }

    public static Customer Create(
        string code, string name, CustomerType customerType,
        string? taxId, string? country, string? address, string? shippingAddress,
        string? contactName, string? contactPhone, string? contactEmail,
        int creditTermsDays, string? currency, string? notes,
        string? createdBy)
    {
        return new Customer
        {
            CustomerCode = code.Trim().ToUpperInvariant(),
            CustomerName = name.Trim(),
            CustomerType = customerType,
            TaxId = taxId?.Trim(),
            Country = country?.Trim(),
            Address = address?.Trim(),
            ShippingAddress = shippingAddress?.Trim(),
            ContactName = contactName?.Trim(),
            ContactPhone = contactPhone?.Trim(),
            ContactEmail = contactEmail?.Trim(),
            CreditTermsDays = creditTermsDays,
            Currency = string.IsNullOrWhiteSpace(currency) ? "VND" : currency.Trim().ToUpperInvariant(),
            Notes = notes?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, CustomerType customerType,
        string? taxId, string? country, string? address, string? shippingAddress,
        string? contactName, string? contactPhone, string? contactEmail,
        int creditTermsDays, string? currency, string? notes,
        bool isActive, string? updatedBy)
    {
        CustomerName = name.Trim();
        CustomerType = customerType;
        TaxId = taxId?.Trim();
        Country = country?.Trim();
        Address = address?.Trim();
        ShippingAddress = shippingAddress?.Trim();
        ContactName = contactName?.Trim();
        ContactPhone = contactPhone?.Trim();
        ContactEmail = contactEmail?.Trim();
        CreditTermsDays = creditTermsDays;
        Currency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        Notes = notes?.Trim();
        IsActive = isActive;
        Touch(updatedBy);
    }

    // ── Part numbers ────────────────────────────────────────────────────────

    public CustomerPartNumber AddPartNumber(
        string customerPartNo, string productCode,
        string? description, string? drawingReference, string? revision)
    {
        var normalizedPartNo = customerPartNo.Trim().ToUpperInvariant();
        if (_partNumbers.Any(x => x.CustomerPartNo == normalizedPartNo))
            throw new DomainException($"Customer part number '{normalizedPartNo}' already exists for this customer.");

        var partNumber = CustomerPartNumber.Create(
            CustomerCode, normalizedPartNo, productCode,
            description, drawingReference, revision);
        _partNumbers.Add(partNumber);
        return partNumber;
    }

    public void UpdatePartNumber(
        int customerPartNumberId,
        string? description, string? drawingReference, string? revision)
    {
        var partNumber = _partNumbers.FirstOrDefault(x => x.CustomerPartNumberId == customerPartNumberId)
            ?? throw new DomainException($"customerPartNumberId '{customerPartNumberId}' was not found.");
        partNumber.Update(description, drawingReference, revision);
    }

    public void RemovePartNumber(int customerPartNumberId)
    {
        var partNumber = _partNumbers.FirstOrDefault(x => x.CustomerPartNumberId == customerPartNumberId)
            ?? throw new DomainException($"customerPartNumberId '{customerPartNumberId}' was not found.");
        _partNumbers.Remove(partNumber);
    }

    // ── Quality specs ───────────────────────────────────────────────────────

    public CustomerQualitySpec SetQualitySpec(
        string productCode,
        string? aqlLevel, InspectionLevel? inspectionLevel,
        string? acceptanceCriteria, int? maxDefectsPpm,
        string? specialRequirements,
        DateOnly? effectiveFrom, DateOnly? effectiveTo)
    {
        var normalizedCode = productCode.Trim().ToUpperInvariant();
        var existing = _qualitySpecs.FirstOrDefault(x => x.ProductCode == normalizedCode);
        if (existing is not null)
        {
            existing.Update(
                aqlLevel, inspectionLevel, acceptanceCriteria, maxDefectsPpm,
                specialRequirements, effectiveFrom, effectiveTo);
            return existing;
        }

        var spec = CustomerQualitySpec.Create(
            CustomerCode, normalizedCode,
            aqlLevel, inspectionLevel, acceptanceCriteria, maxDefectsPpm,
            specialRequirements, effectiveFrom, effectiveTo);
        _qualitySpecs.Add(spec);
        return spec;
    }

    public void RemoveQualitySpec(int customerQualitySpecId)
    {
        var spec = _qualitySpecs.FirstOrDefault(x => x.CustomerQualitySpecId == customerQualitySpecId)
            ?? throw new DomainException($"customerQualitySpecId '{customerQualitySpecId}' was not found.");
        _qualitySpecs.Remove(spec);
    }
}
