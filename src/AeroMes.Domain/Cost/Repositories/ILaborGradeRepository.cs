namespace AeroMes.Domain.Cost.Repositories;

public record LaborGradeDto(
    int LaborGradeID, string GradeCode, string GradeName,
    decimal HourlyRate, decimal OvertimeMultiplier,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, string Currency, DateTime CreatedAt);

public interface ILaborGradeRepository
{
    Task<int> AddAsync(LaborGrade grade, CancellationToken ct);
    Task<LaborGrade?> GetByIdAsync(int id, CancellationToken ct);
    Task<LaborGrade?> GetActiveByCodeAsync(string gradeCode, CancellationToken ct);
    Task<IReadOnlyList<LaborGradeDto>> GetListAsync(string? keyword, bool includeExpired, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
