using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;

public class GetProductAttributeAssignmentsHandler(IProductAttributeRepository repo)
    : IQueryHandler<GetProductAttributeAssignmentsQuery, IReadOnlyList<ProductAttributeAssignmentDto>>
{
    public async Task<IReadOnlyList<ProductAttributeAssignmentDto>> HandleAsync(
        GetProductAttributeAssignmentsQuery q, CancellationToken ct)
    {
        var assignments = await repo.GetAssignmentsForProductAsync(q.ProductCode, ct);
        return [.. assignments.Select(a => new ProductAttributeAssignmentDto(
            a.AssignmentId,
            a.AttributeId,
            a.Attribute?.AttributeCode ?? string.Empty,
            a.Attribute?.AttributeName ?? string.Empty,
            a.SelectedValueId,
            a.SelectedValue?.Value))];
    }
}
