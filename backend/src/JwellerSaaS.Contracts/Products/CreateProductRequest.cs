namespace JwellerSaaS.Contracts.Products;

public sealed record CreateProductRequest(
    long SubCategoryId,
    string ProductCode,
    string ProductName,
    string? Description,
    long? ProductTypeId,
    long? BrandId,
    long? CollectionId,
    bool IsActive,
    string Sku,
    string? VariantName,
    long? UnitId);
