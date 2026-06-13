using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapters;

public record GetAdaptersQuery(string MachineCode) : IQuery<IReadOnlyList<AdapterDto>>;
