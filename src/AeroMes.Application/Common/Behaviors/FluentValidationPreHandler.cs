using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Common.Behaviors;

public sealed class FluentValidationPreHandler<TCommand>(IEnumerable<IValidator<TCommand>> validators)
    : ICommandPreHandler<TCommand>
    where TCommand : ICommand
{
    public async Task PreHandleAsync(TCommand message, CancellationToken cancellationToken = default)
    {
        if (!validators.Any()) return;

        var context = new ValidationContext<TCommand>(message);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);
    }
}
