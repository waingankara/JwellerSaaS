using JwellerSaaS.Contracts.Products;

namespace JwellerSaaS.Application.Products;

public interface IProductService
{
    Task<CreateProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);
}
