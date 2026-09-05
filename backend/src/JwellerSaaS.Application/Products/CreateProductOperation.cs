using JwellerSaaS.Contracts.Products;
using JwellerSaaS.Domain.Masters;
using JwellerSaaS.Domain.Transactions;
using Orion.Framework.Crud;
using Orion.Framework.Transactions;

namespace JwellerSaaS.Application.Products;

/// <summary>
/// Creates a product together with its first sellable variant
/// within a single framework-managed transaction.
/// </summary>
public sealed class CreateProductOperation(
    CreateProductRequest request,
    ICrudService<Product> productCrud,
    ICrudService<ProductVariant> variantCrud
    )
    : ITransactionalOperation<CreateProductResponse>
{
    /// <inheritdoc />
    public async Task<CreateProductResponse> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product =
            new Product
            {
                SubCategoryId = request.SubCategoryId,
                ProductCode = request.ProductCode.Trim(),
                ProductName = request.ProductName.Trim(),
                Description = request.Description,
                ProductTypeId = request.ProductTypeId,
                BrandId = request.BrandId,
                CollectionId = request.CollectionId,
                IsActive = request.IsActive,
                RowVersion = Guid.NewGuid()
            };

        await productCrud
            .CreateAsync(
                product,
                cancellationToken)
            .ConfigureAwait(false);

        var variant =
            new ProductVariant
            {
                ProductId = product.ProductId,
                Sku = request.Sku.Trim(),
                VariantName = request.VariantName?.Trim(),
                UnitId = request.UnitId,
                IsActive = request.IsActive,
                RowVersion = Guid.NewGuid()
            };

        await variantCrud
            .CreateAsync(
                variant,
                cancellationToken)
            .ConfigureAwait(false);

        return new CreateProductResponse(
            product.ProductId,
            variant.ProductVariantId);
    }
}
