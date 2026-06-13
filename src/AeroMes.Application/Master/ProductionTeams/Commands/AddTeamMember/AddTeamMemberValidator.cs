using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductionTeams.Commands.AddTeamMember;

public class AddTeamMemberValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberValidator(IEmployeeRepository employeeRepo)
    {
        RuleFor(x => x.EmployeeCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().MaximumLength(50)
            .MustAsync(employeeRepo.IsActiveAsync)
            .WithMessage(x => $"Nhân viên '{x.EmployeeCode}' không tồn tại hoặc đã ngưng hoạt động.");
    }
}
