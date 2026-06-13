using FluentValidation;

namespace AeroMes.Application.Iot.Signals.Commands.CreateSignalTag;

public class CreateSignalTagValidator : AbstractValidator<CreateSignalTagCommand>
{
    private static readonly string[] ValidCategories = ["MOTION", "ELECTRICAL", "THERMAL", "COUNTER", "STATUS", "CUSTOM"];
    private static readonly string[] ValidDataTypes = ["FLOAT", "INT", "BOOL", "STRING"];

    public CreateSignalTagValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => ValidCategories.Contains(c))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}");

        RuleFor(x => x.DataType)
            .NotEmpty()
            .Must(d => ValidDataTypes.Contains(d))
            .WithMessage($"DataType must be one of: {string.Join(", ", ValidDataTypes)}");
    }
}
