using FluentValidation;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.CreateInspectionRequest;

public class CreateInspectionRequestValidator : AbstractValidator<CreateInspectionRequestCommand>
{
    public CreateInspectionRequestValidator()
    {
        RuleFor(x => x.RequestNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RequesterName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RequestingDepartment).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientPerson).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InspectionDeadline).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
    }
}
