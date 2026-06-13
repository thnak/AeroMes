using AeroMes.Domain.Integration;
using AeroMes.Domain.Reminders;
using AeroMes.Domain.Reminders.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Reminders;

public sealed class ReminderEvaluationService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderEvaluationService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("ReminderEvaluationService started (interval: {Interval})", Interval);
        await Task.Delay(TimeSpan.FromSeconds(30), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await EvaluateAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error evaluating reminders");
            }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task EvaluateAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reminderRepo = scope.ServiceProvider.GetRequiredService<IReminderRepository>();

        var now = DateTime.UtcNow;
        var alertsAdded = 0;

        // Overdue production orders
        var overdueOrders = await db.Set<ProductionOrder>()
            .AsNoTracking()
            .Where(po => po.PlannedEndDate < now
                      && po.Status != ProductionOrderStatus.Completed
                      && po.Status != ProductionOrderStatus.Cancelled)
            .Select(po => new { po.POID, po.POCode, po.PlannedEndDate })
            .Take(200)
            .ToListAsync(ct);

        foreach (var po in overdueOrders)
        {
            if (await reminderRepo.AlertExistsAsync("ProductionOrderOverdue", po.POID.ToString(), ct)) continue;
            var daysOverdue = (int)(now - po.PlannedEndDate!.Value).TotalDays;
            reminderRepo.AddAlert(ReminderAlert.Create(
                "ProductionOrderOverdue", "production-orders", po.POID.ToString(), po.POCode,
                $"Production order {po.POCode} is {daysOverdue} day(s) overdue.",
                "Error"));
            alertsAdded++;
        }

        // Approaching deadline (within 2 days)
        var approaching = now.AddDays(2);
        var approachingOrders = await db.Set<ProductionOrder>()
            .AsNoTracking()
            .Where(po => po.PlannedEndDate >= now
                      && po.PlannedEndDate <= approaching
                      && po.Status != ProductionOrderStatus.Completed
                      && po.Status != ProductionOrderStatus.Cancelled)
            .Select(po => new { po.POID, po.POCode, po.PlannedEndDate })
            .Take(200)
            .ToListAsync(ct);

        foreach (var po in approachingOrders)
        {
            if (await reminderRepo.AlertExistsAsync("ProductionOrderDeadlineApproaching", po.POID.ToString(), ct)) continue;
            var hoursLeft = (int)(po.PlannedEndDate!.Value - now).TotalHours;
            reminderRepo.AddAlert(ReminderAlert.Create(
                "ProductionOrderDeadlineApproaching", "production-orders", po.POID.ToString(), po.POCode,
                $"Production order {po.POCode} deadline in {hoursLeft}h.",
                "Warning"));
            alertsAdded++;
        }

        if (alertsAdded > 0) await db.SaveChangesAsync(ct);
        logger.LogDebug("Reminder evaluation complete: {Added} alerts created", alertsAdded);
    }
}
