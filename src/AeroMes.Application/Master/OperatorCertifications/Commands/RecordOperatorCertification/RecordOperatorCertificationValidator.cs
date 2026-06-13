using FluentValidation;

namespace AeroMes.Application.Master.OperatorCertifications.Commands.RecordOperatorCertification;

public class RecordOperatorCertificationValidator : AbstractValidator<RecordOperatorCertificationCommand>
{
    public RecordOperatorCertificationValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CertificationCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.IssuedDate).NotEmpty();
        RuleFor(x => x.IssuedBy).MaximumLength(100);
        RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(x => x.IssuedDate)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Ngày hết hạn phải sau ngày cấp.");
    }
}
