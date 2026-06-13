using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;

public record GetAdapterDetailQuery(int Id) : IQuery<AdapterDetailDto?>;
