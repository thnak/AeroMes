using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateMaterialPurchaseRequest;

public record PurchaseRequestLineInput(
    string MaterialCode,
    string MaterialName,
    string UnitOfMeasure,
    decimal RequiredQty,
    decimal? Length = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Radius = null,
    decimal? Weight = null);

public record CreateMaterialPurchaseRequestCommand(
    string Requestor,
    string? RequestingUnit,
    DateOnly? Deadline,
    string? ProcurementPurpose,
    PurchaseRequestSourceType SourceType,
    int? SourceReferenceId,
    string? SalesOrderCode,
    IReadOnlyList<PurchaseRequestLineInput> Lines) : ICommand<ValidationResult<int>>;
