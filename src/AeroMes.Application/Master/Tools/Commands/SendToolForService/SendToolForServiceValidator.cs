using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.SendToolForService;

public class SendToolForServiceValidator : AbstractValidator<SendToolForServiceCommand>
{
    public SendToolForServiceValidator()
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.ServiceType).IsInEnum();
    }
}
