using AeroMes.Application.Master.Products.Queries.GetProducts;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductByCode;

public record GetProductByCodeQuery(string Code) : IQuery<ProductDetailDto?>;
