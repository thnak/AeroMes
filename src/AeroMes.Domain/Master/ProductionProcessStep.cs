using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class ProductionProcessStep : AuditableEntity
{
    public int StepID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProcessApplicationScope ApplicationScope { get; private set; } = ProcessApplicationScope.All;
    public string? ProductGroupIdsJson { get; private set; }
    public string? ProductIdsJson { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ProductionProcessStep() { }

    public static ProductionProcessStep Create(
        string code, string name, string? description,
        ProcessApplicationScope scope, string? productGroupIdsJson, string? productIdsJson,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã công đoạn không được để trống.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên công đoạn không được để trống.");
        return new ProductionProcessStep
        {
            Code = code.Trim(), Name = name.Trim(), Description = description,
            ApplicationScope = scope, ProductGroupIdsJson = productGroupIdsJson,
            ProductIdsJson = productIdsJson, CreatedBy = createdBy
        };
    }

    public void Update(
        string name, string? description,
        ProcessApplicationScope scope, string? productGroupIdsJson,
        string? productIdsJson, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên công đoạn không được để trống.");
        Name = name.Trim();
        Description = description;
        ApplicationScope = scope;
        ProductGroupIdsJson = productGroupIdsJson;
        ProductIdsJson = productIdsJson;
        Touch(updatedBy);
    }

    public void Activate(string? updatedBy) { IsActive = true; Touch(updatedBy); }
    public void Deactivate(string? updatedBy) { IsActive = false; Touch(updatedBy); }
}
