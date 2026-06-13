using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateStockPolicy;

public class CreateStockPolicyValidator : AbstractValidator<CreateStockPolicyCommand>
{
    public CreateStockPolicyValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().WithMessage("Mã sản phẩm là bắt buộc.");
        RuleFor(x => x.LocationId).GreaterThan(0).WithMessage("Vị trí lưu kho là bắt buộc.");
        RuleFor(x => x.MinQty).GreaterThanOrEqualTo(0).WithMessage("Mức tồn tối thiểu không được âm.");
        RuleFor(x => x.MaxQty).GreaterThan(0).WithMessage("Mức tồn tối đa phải lớn hơn 0.");
        RuleFor(x => x.ReorderQty).GreaterThan(0).WithMessage("Số lượng đặt hàng phải lớn hơn 0.");
        RuleFor(x => x.LeadTimeDays).GreaterThanOrEqualTo(0).WithMessage("Thời gian giao hàng không được âm.");
    }
}
