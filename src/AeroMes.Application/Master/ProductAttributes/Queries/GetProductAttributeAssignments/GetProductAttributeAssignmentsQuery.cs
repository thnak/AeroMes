using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;

public record GetProductAttributeAssignmentsQuery(string ProductCode)
    : IQuery<IReadOnlyList<ProductAttributeAssignmentDto>>;

public record ProductAttributeAssignmentDto(
    int AssignmentId,
    int AttributeId,
    string AttributeCode,
    string AttributeName,
    int? SelectedValueId,
    string? SelectedValue);
