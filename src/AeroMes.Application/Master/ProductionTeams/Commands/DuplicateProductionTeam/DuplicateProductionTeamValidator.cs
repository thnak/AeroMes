using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DuplicateProductionTeam;

public class DuplicateProductionTeamValidator : AbstractValidator<DuplicateProductionTeamCommand>
{
    public DuplicateProductionTeamValidator(IProductionTeamRepository repo)
    {
        RuleFor(x => x.NewCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().MaximumLength(50)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Mã tổ sản xuất '{x.NewCode}' đã tồn tại.");
    }
}
