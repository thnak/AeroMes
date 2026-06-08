using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Routings.Commands.CreateRouting;

public class CreateRoutingValidator : AbstractValidator<CreateRoutingCommand>
{
    public CreateRoutingValidator(IRoutingRepository routingRepo, IProductRepository productRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code must be at most 20 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await routingRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Routing code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required.")
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product '{x.ProductCode}' does not exist.");
    }
}
