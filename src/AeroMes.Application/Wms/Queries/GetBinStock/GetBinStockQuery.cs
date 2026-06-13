using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBinStock;

public record GetBinStockQuery(int BinId) : IQuery<IReadOnlyList<BinStockDto>>;

public record BinStockDto(string ProductCode, string LotNumber, decimal Quantity, DateTime UpdatedAt);
