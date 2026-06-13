using FluentValidation;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.CreateInspectionVoucher;

public class CreateInspectionVoucherValidator : AbstractValidator<CreateInspectionVoucherCommand>
{
    public CreateInspectionVoucherValidator()
    {
        RuleFor(x => x.VoucherNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VoucherName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InspectorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SampleQuantity).GreaterThan(0);
    }
}
