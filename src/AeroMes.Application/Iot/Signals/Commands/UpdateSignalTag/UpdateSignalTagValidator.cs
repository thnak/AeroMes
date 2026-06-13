using FluentValidation;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignalTag;

public class UpdateSignalTagValidator : AbstractValidator<UpdateSignalTagCommand>
{
    private static readonly string[] ValidCategories = ["MOTION", "ELECTRICAL", "THERMAL", "COUNTER", "STATUS", "CUSTOM"];
    private static readonly string[] ValidDataTypes = ["FLOAT", "INT", "BOOL", "STRING"];

    public UpdateSignalTagValidator()
    {
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
