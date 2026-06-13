using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OperatorCertifications.Queries.CheckOperatorEligibility;

public class CheckOperatorEligibilityHandler(
    IMachineRepository machineRepo,
    IOperatorCertificationRepository certRepo) : IQueryHandler<CheckOperatorEligibilityQuery, OperatorEligibilityResult>
{
    public async Task<OperatorEligibilityResult> HandleAsync(CheckOperatorEligibilityQuery q, CancellationToken ct)
    {
        var machine = await machineRepo.GetByCodeAsync(q.MachineCode, ct);
        if (machine is null)
            return new OperatorEligibilityResult(false, "Máy không tồn tại.", null, null);

        if (!machine.RequiresCertification || string.IsNullOrEmpty(machine.CertificationCode))
            return new OperatorEligibilityResult(true, "Máy không yêu cầu chứng chỉ.", null, null);

        var cert = await certRepo.GetActiveAsync(q.EmployeeCode, machine.CertificationCode, ct);
        if (cert is null)
            return new OperatorEligibilityResult(false, $"Không có chứng chỉ '{machine.CertificationCode}' còn hiệu lực.", machine.CertificationCode, null);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (cert.ExpiryDate.HasValue && cert.ExpiryDate.Value < today)
            return new OperatorEligibilityResult(false, $"Chứng chỉ '{machine.CertificationCode}' đã hết hạn.", machine.CertificationCode, cert.ExpiryDate);

        return new OperatorEligibilityResult(true, "Đủ điều kiện vận hành.", machine.CertificationCode, cert.ExpiryDate);
    }
}
