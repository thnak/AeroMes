using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetDashboardLayout;

public record GetDashboardLayoutQuery(string UserId) : IQuery<string?>;

public class GetDashboardLayoutQueryHandler(IDashboardLayoutRepository repo)
    : IQueryHandler<GetDashboardLayoutQuery, string?>
{
    public async Task<string?> HandleAsync(GetDashboardLayoutQuery query, CancellationToken ct = default)
    {
        var layout = await repo.GetByUserIdAsync(query.UserId, ct);
        return layout?.LayoutJson;
    }
}
