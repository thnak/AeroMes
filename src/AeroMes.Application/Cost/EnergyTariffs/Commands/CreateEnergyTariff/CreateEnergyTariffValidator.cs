using AeroMes.Domain.Cost;
using FluentValidation;

namespace AeroMes.Application.Cost.EnergyTariffs.Commands.CreateEnergyTariff;

public class CreateEnergyTariffValidator : AbstractValidator<CreateEnergyTariffCommand>
{
    public CreateEnergyTariffValidator()
    {
        RuleFor(x => x.TariffName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PeakRateKWh).GreaterThan(0);
        When(x => x.TariffType == EnergyTariffType.TIME_OF_USE, () =>
        {
            RuleFor(x => x.OffPeakRateKWh).NotNull().GreaterThan(0)
                .WithMessage("Giá điện ngoài giờ cao điểm là bắt buộc với biểu giá TOU.");
            RuleFor(x => x.PeakHourStart).NotNull()
                .WithMessage("Giờ bắt đầu cao điểm là bắt buộc với biểu giá TOU.");
            RuleFor(x => x.PeakHourEnd).NotNull()
                .WithMessage("Giờ kết thúc cao điểm là bắt buộc với biểu giá TOU.");
        });
    }
}
