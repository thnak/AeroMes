using System.Text.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.GenerateVariantMatrix;

public sealed class GenerateVariantMatrixHandler(
    IProductFamilyRepository repo,
    IProductRepository products,
    IValidator<GenerateVariantMatrixCommand> validator,
    IUnitOfWork uow) : ICommandHandler<GenerateVariantMatrixCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(GenerateVariantMatrixCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        var family = await repo.GetWithDimensionsAsync(cmd.FamilyCode.ToUpperInvariant(), ct);
        if (family is null) return ValidationResult<int>.NotFound($"Family '{cmd.FamilyCode}' not found.");

        var dimensions = family.Dimensions
            .Where(d => d.IsRequired)
            .OrderBy(d => d.SortOrder)
            .ToList();

        if (dimensions.Count == 0)
            return ValidationResult<int>.Failure("Family has no required dimensions to generate variants from.");

        // Build cross-product of active dimension values
        var combinations = CrossProduct(dimensions.Select(d =>
            d.Values.Where(v => v.IsActive).Select(v => (d.DimensionName, v.ValueCode)).ToList()));

        var baseProduct = await products.GetByCodeAsync(family.BaseProductCode, ct);
        if (baseProduct is null)
            return ValidationResult<int>.NotFound($"Base product '{family.BaseProductCode}' not found.");

        int created = 0;
        foreach (var combo in combinations)
        {
            var variantKey = string.Join("|", combo.Select(c => c.ValueCode));
            var existing = await repo.GetVariantByKeyAsync(family.FamilyCode, variantKey, ct);
            if (existing is not null) continue;

            // Generate product code: prefix + dimension values separated by -
            var suffix = string.Join("-", combo.Select(c => c.ValueCode));
            var productCode = $"{cmd.ProductCodePrefix}-{suffix}".ToUpperInvariant();
            productCode = productCode[..Math.Min(productCode.Length, 50)];

            var attrJson = JsonSerializer.Serialize(
                combo.ToDictionary(c => c.DimensionName, c => c.ValueCode));

            // Create the product if it doesn't exist
            var existingProduct = await products.GetByCodeAsync(productCode, ct);
            if (existingProduct is null)
            {
                var variantProduct = Product.Create(
                    productCode,
                    $"{family.FamilyName} ({suffix})",
                    baseProduct.BaseUoMCode,
                    baseProduct.ItemType,
                    baseProduct.CategoryId,
                    null, // barcodePattern
                    baseProduct.LotControlled,
                    baseProduct.SerialControlled,
                    baseProduct.ShelfLifeDays,
                    baseProduct.ProcurementType,
                    null, null, null, cmd.CreatedBy);
                await products.AddAsync(variantProduct, ct);
            }

            var variant = ProductVariant.Create(family.FamilyCode, productCode, variantKey, attrJson);
            await repo.AddVariantAsync(variant, ct);
            created++;
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(created);
    }

    private static IEnumerable<List<(string DimensionName, string ValueCode)>> CrossProduct(
        IEnumerable<List<(string DimensionName, string ValueCode)>> dimensions)
    {
        IEnumerable<List<(string DimensionName, string ValueCode)>> result = [[]];
        foreach (var dim in dimensions)
        {
            result = result.SelectMany(combo => dim.Select(val =>
                new List<(string, string)>([..combo, val])));
        }
        return result;
    }
}
