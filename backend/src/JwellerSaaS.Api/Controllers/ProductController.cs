using JwellerSaaS.Application.Products;
using JwellerSaaS.Contracts.Products;
using Microsoft.AspNetCore.Mvc;

namespace JwellerSaaS.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductController(
    IProductService productService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await productService.CreateAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}
