using AeroMes.Domain.Alert;

namespace AeroMes.Application.Interfaces;

public interface IAlertNotifier
{
    Task PushAsync(AlertEventDto alert, CancellationToken ct = default);
}
