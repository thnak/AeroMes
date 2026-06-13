using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductionTeams.Commands.CreateProductionTeam;

public class CreateProductionTeamValidator : AbstractValidator<CreateProductionTeamCommand>
{
    public CreateProductionTeamValidator(
        IProductionTeamRepository teamRepo,
        IOrgUnitRepository orgUnitRepo,
        IProductCategoryRepository categoryRepo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().MaximumLength(50)
            .MustAsync(async (code, ct) => !await teamRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Mã tổ sản xuất '{x.Code}' đã tồn tại.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StandardLaborQuantity).GreaterThan(0).When(x => x.StandardLaborQuantity is not null);
        RuleFor(x => x.ProductionRate).GreaterThan(0).When(x => x.ProductionRate is not null);

        RuleFor(x => x.OrgUnitId!.Value)
            .MustAsync(orgUnitRepo.IsActiveAsync)
            .When(x => x.OrgUnitId is not null)
            .WithMessage(x => $"Đơn vị tổ chức #{x.OrgUnitId} không tồn tại hoặc đã ngưng hoạt động.");

        RuleForEach(x => x.ProductGroupCategoryIds)
            .MustAsync(categoryRepo.IsActiveAsync)
            .WithMessage((_, id) => $"Nhóm sản phẩm #{id} không tồn tại hoặc đã ngưng hoạt động.");
    }
}
