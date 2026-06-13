using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum StandardSetStatus { Active, Discontinued }

public class QualityStandardSet : AuditableEntity
{
    public int StandardSetID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int SamplingMethodID { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public string? Notes { get; private set; }
    public int? ProductionProcessId { get; private set; }
    public StandardSetStatus Status { get; private set; } = StandardSetStatus.Active;

    private readonly List<QualityStandardSetCriteria> _productCriteria = [];
    public IReadOnlyList<QualityStandardSetCriteria> ProductCriteria => _productCriteria.AsReadOnly();

    private readonly List<QualityStandardSetStageCriteria> _stageCriteria = [];
    public IReadOnlyList<QualityStandardSetStageCriteria> StageCriteria => _stageCriteria.AsReadOnly();

    private QualityStandardSet() { }

    public static QualityStandardSet Create(
        string code, string name, string productCode,
        int samplingMethodId, DateOnly effectiveDate,
        string? notes, int? productionProcessId, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã bộ tiêu chuẩn không được để trống.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên bộ tiêu chuẩn không được để trống.");
        if (string.IsNullOrWhiteSpace(productCode)) throw new DomainException("Sản phẩm không được để trống.");
        return new QualityStandardSet
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ProductCode = productCode.Trim(),
            SamplingMethodID = samplingMethodId,
            EffectiveDate = effectiveDate,
            Notes = notes?.Trim(),
            ProductionProcessId = productionProcessId,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name, int samplingMethodId, DateOnly effectiveDate,
        string? notes, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên bộ tiêu chuẩn không được để trống.");
        Name = name.Trim();
        SamplingMethodID = samplingMethodId;
        EffectiveDate = effectiveDate;
        Notes = notes?.Trim();
        Touch(updatedBy);
    }

    public void Activate(string? updatedBy) { Status = StandardSetStatus.Active; Touch(updatedBy); }
    public void Discontinue(string? updatedBy) { Status = StandardSetStatus.Discontinued; Touch(updatedBy); }

    public void SetProductCriteria(IEnumerable<(int CriteriaId, string? Parameters)> criteria, string? updatedBy)
    {
        _productCriteria.Clear();
        foreach (var (criteriaId, parameters) in criteria)
            _productCriteria.Add(new QualityStandardSetCriteria(StandardSetID, criteriaId, parameters));
        Touch(updatedBy);
    }

    public void SetStageCriteria(IEnumerable<(int StageId, int CriteriaId, int? SamplingMethodId, string? Parameters)> criteria, string? updatedBy)
    {
        _stageCriteria.Clear();
        foreach (var (stageId, criteriaId, samplingMethodId, parameters) in criteria)
            _stageCriteria.Add(new QualityStandardSetStageCriteria(StandardSetID, stageId, criteriaId, samplingMethodId, parameters));
        Touch(updatedBy);
    }
}

public class QualityStandardSetCriteria
{
    public int ID { get; private set; }
    public int StandardSetID { get; private set; }
    public int CriteriaID { get; private set; }
    public string? Parameters { get; private set; }

    private QualityStandardSetCriteria() { }

    public QualityStandardSetCriteria(int standardSetId, int criteriaId, string? parameters)
    {
        StandardSetID = standardSetId;
        CriteriaID = criteriaId;
        Parameters = parameters;
    }
}

public class QualityStandardSetStageCriteria
{
    public int ID { get; private set; }
    public int StandardSetID { get; private set; }
    public int ProductionStageID { get; private set; }
    public int CriteriaID { get; private set; }
    public int? SamplingMethodID { get; private set; }
    public string? Parameters { get; private set; }

    private QualityStandardSetStageCriteria() { }

    public QualityStandardSetStageCriteria(int standardSetId, int stageId, int criteriaId, int? samplingMethodId, string? parameters)
    {
        StandardSetID = standardSetId;
        ProductionStageID = stageId;
        CriteriaID = criteriaId;
        SamplingMethodID = samplingMethodId;
        Parameters = parameters;
    }
}
