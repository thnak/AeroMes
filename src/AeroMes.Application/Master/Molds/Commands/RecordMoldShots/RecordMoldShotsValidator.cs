using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.RecordMoldShots;

public class RecordMoldShotsValidator : AbstractValidator<RecordMoldShotsCommand>
{
    public RecordMoldShotsValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty();
        RuleFor(x => x.Shots).GreaterThan(0);
    }
}
