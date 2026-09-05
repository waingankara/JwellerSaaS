using JwellerSaaS.Contracts.Products;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Transactions;

namespace JwellerSaaS.Application.Products;

public sealed class ProductService(
    ITransactionalOperationExecutor transactionExecutor,
    IServiceProvider serviceProvider)
    : IProductService
{
    public Task<CreateProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operation = ActivatorUtilities.CreateInstance<CreateProductOperation>(
            serviceProvider,
            request);

        return transactionExecutor.ExecuteAsync(
            operation,
            cancellationToken);
    }
}
