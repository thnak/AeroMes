namespace AeroMes.Domain.Labels.Repositories;

public interface ILabelRepository
{
    Task<List<LabelTemplate>> GetTemplatesAsync(CancellationToken ct = default);
    Task<LabelTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default);
    Task<LabelTemplate?> GetDefaultTemplateAsync(CancellationToken ct = default);
    void AddTemplate(LabelTemplate template);
    void RemoveTemplate(LabelTemplate template);

    Task ClearDefaultAsync(CancellationToken ct = default);

    Task<List<LabelPrintJob>> GetPrintJobsAsync(string? entityType, string? entityId, CancellationToken ct = default);
    void AddPrintJob(LabelPrintJob job);
}
