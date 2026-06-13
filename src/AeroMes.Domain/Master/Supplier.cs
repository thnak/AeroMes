using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Supplier : AuditableEntity
{
    public string SupplierCode { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? ContactName { get; private set; }
    public string? TaxCode { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ApprovedVendorItem> _avlItems = [];
    public IReadOnlyList<ApprovedVendorItem> AvlItems => _avlItems.AsReadOnly();

    private Supplier() { }

    public static Supplier Create(
        string code, string name,
        string? country, string? city, string? address,
        string? phone, string? email, string? contactName, string? taxCode,
        string? createdBy)
    {
        return new Supplier
        {
            SupplierCode = code.Trim().ToUpperInvariant(),
            SupplierName = name.Trim(),
            Country = country?.Trim(),
            City = city?.Trim(),
            Address = address?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            ContactName = contactName?.Trim(),
            TaxCode = taxCode?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, string? country, string? city, string? address,
        string? phone, string? email, string? contactName, string? taxCode,
        bool isActive, string? updatedBy)
    {
        SupplierName = name.Trim();
        Country = country?.Trim();
        City = city?.Trim();
        Address = address?.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        ContactName = contactName?.Trim();
        TaxCode = taxCode?.Trim();
        IsActive = isActive;
        Touch(updatedBy);
    }

    public ApprovedVendorItem AddAvlItem(
        string productCode, AvlStatus status,
        decimal? unitPrice, string? currencyCode, int? leadTimeDays,
        decimal? minOrderQty, string? aqlLevel, bool isPreferred,
        DateOnly? approvedFrom, DateOnly? approvedTo, string? notes)
    {
        var normalizedCode = productCode.Trim().ToUpperInvariant();
        if (_avlItems.Any(x => x.ProductCode == normalizedCode))
            throw new DomainException($"Product '{normalizedCode}' is already in the AVL for this supplier.");

        var item = ApprovedVendorItem.Create(
            SupplierCode, normalizedCode, status,
            unitPrice, currencyCode, leadTimeDays,
            minOrderQty, aqlLevel, isPreferred,
            approvedFrom, approvedTo, notes);
        _avlItems.Add(item);
        return item;
    }

    public void UpdateAvlItem(
        int avlItemId, AvlStatus status, decimal? unitPrice, string? currencyCode,
        int? leadTimeDays, decimal? minOrderQty, string? aqlLevel, bool isPreferred,
        DateOnly? approvedFrom, DateOnly? approvedTo, string? notes)
    {
        var item = _avlItems.FirstOrDefault(x => x.AvlItemId == avlItemId)
            ?? throw new DomainException($"avlItemId '{avlItemId}' was not found.");
        item.Update(status, unitPrice, currencyCode, leadTimeDays, minOrderQty, aqlLevel, isPreferred, approvedFrom, approvedTo, notes);
    }

    public void RemoveAvlItem(int avlItemId)
    {
        var item = _avlItems.FirstOrDefault(x => x.AvlItemId == avlItemId)
            ?? throw new DomainException($"avlItemId '{avlItemId}' was not found.");
        _avlItems.Remove(item);
    }
}
