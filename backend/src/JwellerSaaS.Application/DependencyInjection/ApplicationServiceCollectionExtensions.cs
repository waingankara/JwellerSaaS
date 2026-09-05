using JwellerSaaS.Application.Masters;
using JwellerSaaS.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace JwellerSaaS.Application.DependencyInjection;

/// <summary>Registers application-layer services.</summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Adds application-layer dependencies.</summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<ICategoryBusinessService, CategoryBusinessService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
