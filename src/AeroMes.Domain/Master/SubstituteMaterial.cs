using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum SubstituteMaterialStatus { Active, Inactive }

public class SubstituteMaterial : AuditableEntity
{
    public int SubstituteId { get; private set; }
    public string SubstituteCode { get; private set; } = string.Empty;
    public string PrimaryMaterialCode { get; private set; } = string.Empty;
    public string SubstituteMaterialCode { get; private set; } = string.Empty;
    public decimal ConversionRatio { get; private set; } = 1m;
    public int Priority { get; private set; }
    public SubstituteMaterialStatus Status { get; private set; } = SubstituteMaterialStatus.Active;
    public string? Notes { get; private set; }
    public DateOnly? EffectiveDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    public Product? PrimaryMaterial { get; private set; }
    public Product? SubstituteMaterialProduct { get; private set; }

    private SubstituteMaterial() { }

    public static SubstituteMaterial Create(
        string substituteCode,
        string primaryMaterialCode,
        string substituteMaterialCode,
        decimal conversionRatio,
        int priority,
        string? notes,
        DateOnly? effectiveDate,
        DateOnly? expiryDate,
        string? createdBy)
    {
        primaryMaterialCode = primaryMaterialCode.Trim().ToUpperInvariant();
        substituteMaterialCode = substituteMaterialCode.Trim().ToUpperInvariant();

        if (primaryMaterialCode == substituteMaterialCode)
            throw new DomainException("Nguyên vật liệu thay thế không được trùng với nguyên vật liệu gốc.");
        if (conversionRatio <= 0)
            throw new DomainException("Tỷ lệ quy đổi phải lớn hơn 0.");
        if (effectiveDate.HasValue && expiryDate.HasValue && effectiveDate > expiryDate)
            throw new DomainException("Ngày hiệu lực không được sau ngày hết hạn.");

        return new SubstituteMaterial
        {
            SubstituteCode = substituteCode.Trim().ToUpperInvariant(),
            PrimaryMaterialCode = primaryMaterialCode,
            SubstituteMaterialCode = substituteMaterialCode,
            ConversionRatio = conversionRatio,
            Priority = priority,
            Status = SubstituteMaterialStatus.Active,
            Notes = notes,
            EffectiveDate = effectiveDate,
            ExpiryDate = expiryDate,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        decimal conversionRatio, int priority, string? notes,
        DateOnly? effectiveDate, DateOnly? expiryDate, string? updatedBy)
    {
        if (conversionRatio <= 0)
            throw new DomainException("Tỷ lệ quy đổi phải lớn hơn 0.");
        if (effectiveDate.HasValue && expiryDate.HasValue && effectiveDate > expiryDate)
            throw new DomainException("Ngày hiệu lực không được sau ngày hết hạn.");

        ConversionRatio = conversionRatio;
        Priority = priority;
        Notes = notes;
        EffectiveDate = effectiveDate;
        ExpiryDate = expiryDate;
        Touch(updatedBy);
    }

    public void Activate(string? updatedBy) { Status = SubstituteMaterialStatus.Active; Touch(updatedBy); }
    public void Deactivate(string? updatedBy) { Status = SubstituteMaterialStatus.Inactive; Touch(updatedBy); }
}
