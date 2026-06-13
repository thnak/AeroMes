using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineProductParamRepository(AppDbContext db) : IMachineProductParamRepository
{
    public Task<MachineProductParam?> GetAsync(string machineCode, string productCode, string paramName, CancellationToken ct) =>
        db.MachineProductParams.FirstOrDefaultAsync(
            x => x.MachineCode == machineCode.ToUpperInvariant()
              && x.ProductCode == productCode.ToUpperInvariant()
              && x.ParamName == paramName, ct);

    public async Task<IReadOnlyList<MachineProductParam>> GetByMachineAndProductAsync(
        string machineCode, string productCode, CancellationToken ct) =>
        await db.MachineProductParams
            .Where(x => x.MachineCode == machineCode.ToUpperInvariant()
                     && x.ProductCode == productCode.ToUpperInvariant())
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.ParamName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MachineProductParam>> GetByMachineAsync(string machineCode, CancellationToken ct) =>
        await db.MachineProductParams
            .Where(x => x.MachineCode == machineCode.ToUpperInvariant())
            .OrderBy(x => x.ProductCode).ThenBy(x => x.DisplayOrder).ThenBy(x => x.ParamName)
            .ToListAsync(ct);

    public Task AddAsync(MachineProductParam entity, CancellationToken ct)
    {
        db.MachineProductParams.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MachineProductParam entity) =>
        db.MachineProductParams.Remove(entity);
}
