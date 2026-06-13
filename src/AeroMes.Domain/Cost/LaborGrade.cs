using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public class LaborGrade : Entity
{
    public int LaborGradeID { get; private set; }
    public string GradeCode { get; private set; } = string.Empty;
    public string GradeName { get; private set; } = string.Empty;
    public decimal HourlyRate { get; private set; }
    public decimal OvertimeMultiplier { get; private set; } = 1.5m;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string Currency { get; private set; } = "VND";
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private LaborGrade() { }

    public static LaborGrade Create(
        string gradeCode, string gradeName, decimal hourlyRate,
        decimal overtimeMultiplier, DateOnly effectiveFrom, string currency, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(gradeCode)) throw new DomainException("Mã bậc lương không được để trống.");
        if (hourlyRate <= 0) throw new DomainException("Đơn giá giờ phải lớn hơn 0.");
        if (overtimeMultiplier < 1.0m) throw new DomainException("Hệ số tăng ca phải >= 1.0.");
        return new LaborGrade
        {
            GradeCode = gradeCode.Trim(), GradeName = gradeName.Trim(),
            HourlyRate = hourlyRate, OvertimeMultiplier = overtimeMultiplier,
            EffectiveFrom = effectiveFrom, Currency = currency, CreatedBy = createdBy
        };
    }

    public void ExpireOn(DateOnly expiryDate)
    {
        EffectiveTo = expiryDate;
    }
}
