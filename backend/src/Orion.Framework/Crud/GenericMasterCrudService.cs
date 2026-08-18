using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Search;

namespace Orion.Framework.Crud;

/// <summary>Executes metadata-driven CRUD for registered masters resolved at runtime.</summary>
public interface IGenericMasterCrudService
{
    Task<object> CreateAsync(string master, JsonElement payload, CancellationToken cancellationToken);
    Task<object?> GetAsync(string master, object key, CancellationToken cancellationToken);
    Task<object> UpdateAsync(string master, object key, JsonElement payload, CancellationToken cancellationToken);
    Task<int> DeleteAsync(string master, object key, CancellationToken cancellationToken);
    Task<int> SoftDeleteAsync(string master, object key, CancellationToken cancellationToken);
    Task<int> RestoreAsync(string master, object key, CancellationToken cancellationToken);
    Task<IReadOnlyList<object>> SearchAsync(string master, JsonElement payload, CancellationToken cancellationToken);
    Task<long> CountAsync(string master, JsonElement payload, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string master, object key, CancellationToken cancellationToken);
    Task<IReadOnlyList<object>> DropdownAsync(string master, bool activeOnly, string? searchText, int top, CancellationToken cancellationToken);
}

/// <summary>Default metadata-driven master CRUD service.</summary>
public sealed class GenericMasterCrudService(IMasterRegistry registry, IServiceProvider services) : IGenericMasterCrudService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public async Task<object> CreateAsync(string master, JsonElement payload, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        var entity = Deserialize(definition, payload);
        return await InvokeAsync<object>(definition, nameof(ICrudService<object>.CreateAsync), [entity, cancellationToken]).ConfigureAwait(false) ?? entity;
    }

    public Task<object?> GetAsync(string master, object key, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        return InvokeAsync<object?>(definition, nameof(ICrudService<object>.GetByIdAsync), [ConvertKey(definition, key), cancellationToken]);
    }

    public async Task<object> UpdateAsync(string master, object key, JsonElement payload, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        var entity = Deserialize(definition, payload);
        SetPrimaryKey(definition, entity, key);
        return await InvokeAsync<object>(definition, nameof(ICrudService<object>.UpdateAsync), [entity, cancellationToken]).ConfigureAwait(false) ?? entity;
    }

    public Task<int> DeleteAsync(string master, object key, CancellationToken cancellationToken) => RowsAsync(master, nameof(ICrudService<object>.DeleteAsync), key, cancellationToken);
    public Task<int> SoftDeleteAsync(string master, object key, CancellationToken cancellationToken) => RowsAsync(master, nameof(ICrudService<object>.SoftDeleteAsync), key, cancellationToken);
    public Task<int> RestoreAsync(string master, object key, CancellationToken cancellationToken) => RowsAsync(master, nameof(ICrudService<object>.RestoreAsync), key, cancellationToken);

    public async Task<IReadOnlyList<object>> SearchAsync(string master, JsonElement payload, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        var rows = await InvokeAsync<object>(definition, nameof(ICrudService<object>.GetManyAsync), [BuildQuery(definition, payload, false), cancellationToken]).ConfigureAwait(false);
        return rows is System.Collections.IEnumerable enumerable ? enumerable.Cast<object>().ToArray() : Array.Empty<object>();
    }

    public Task<long> CountAsync(string master, JsonElement payload, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        return InvokeAsync<long>(definition, nameof(ICrudService<object>.CountAsync), [BuildQuery(definition, payload, true), cancellationToken]);
    }

    public Task<bool> ExistsAsync(string master, object key, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        return InvokeAsync<bool>(definition, nameof(ICrudService<object>.ExistsAsync), [ConvertKey(definition, key), cancellationToken]);
    }

    public async Task<IReadOnlyList<object>> DropdownAsync(string master, bool activeOnly, string? searchText, int top, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        var filters = new List<FilterDefinition>();
        AddBooleanFilter(definition, filters, "IsActive", activeOnly ? true : null);
        AddBooleanFilter(definition, filters, "IsDeleted", false);
        var sorts = definition.Columns.Where(c => c.IsDropdownColumn).Select(c => new SortDefinition(c.PropertyName)).DefaultIfEmpty(new SortDefinition(definition.Columns.First(c => !c.IsPrimaryKey).PropertyName)).ToArray();
        var searchFields = definition.Columns.Where(c => c.IsDropdownColumn || c.IsSearchable).Select(c => c.PropertyName).ToArray();
        var query = new QueryDefinition(filters, sorts, new QueryOptions(Top: Math.Clamp(top, 1, 100)), string.IsNullOrWhiteSpace(searchText) ? null : new SearchOptions(searchText, searchFields));
        var rows = await InvokeAsync<object>(definition, nameof(ICrudService<object>.GetManyAsync), [query, cancellationToken]).ConfigureAwait(false);
        return rows is System.Collections.IEnumerable enumerable ? enumerable.Cast<object>().ToArray() : Array.Empty<object>();
    }

    private async Task<int> RowsAsync(string master, string method, object key, CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);
        return await InvokeAsync<int>(definition, method, [ConvertKey(definition, key), cancellationToken]).ConfigureAwait(false);
    }

    private MasterDefinition GetDefinition(string master) => registry.Find(master) ?? throw new KeyNotFoundException($"Master '{master}' is not registered.");

    private object Deserialize(MasterDefinition definition, JsonElement payload) => JsonSerializer.Deserialize(payload.GetRawText(), definition.EntityType, JsonOptions) ?? throw new InvalidOperationException($"Invalid {definition.EntityName} payload.");

    private object? ConvertKey(MasterDefinition definition, object key)
    {
        var keyColumn = definition.Columns.FirstOrDefault(c => c.IsPrimaryKey) ?? throw new InvalidOperationException($"Master '{definition.EntityName}' has no primary key.");
        return Convert.ChangeType(key, Nullable.GetUnderlyingType(keyColumn.PropertyType) ?? keyColumn.PropertyType);
    }

    private void SetPrimaryKey(MasterDefinition definition, object entity, object key)
    {
        var keyColumn = definition.Columns.FirstOrDefault(c => c.IsPrimaryKey) ?? throw new InvalidOperationException($"Master '{definition.EntityName}' has no primary key.");
        definition.EntityType.GetProperty(keyColumn.PropertyName)!.SetValue(entity, ConvertKey(definition, key));
    }

    private async Task<T?> InvokeAsync<T>(MasterDefinition definition, string methodName, object?[] args)
    {
        var serviceType = typeof(ICrudService<>).MakeGenericType(definition.EntityType);
        var service = services.GetRequiredService(serviceType);
        var method = serviceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public) ?? throw new MissingMethodException(serviceType.Name, methodName);
        var task = (Task)method.Invoke(service, args)!;
        await task.ConfigureAwait(false);
        return task.GetType().IsGenericType ? (T?)task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task) : default;
    }

    private static QueryDefinition BuildQuery(MasterDefinition definition, JsonElement payload, bool countOnly)
    {
        var filters = new List<FilterDefinition>();
        var page = 1;
        var pageSize = 20;
        string? sortBy = null;
        var descending = false;
        string? keyword = null;

        if (payload.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in payload.EnumerateObject())
            {
                if (property.NameEquals("page")) page = property.Value.GetInt32();
                else if (property.NameEquals("pageSize")) pageSize = property.Value.GetInt32();
                else if (property.NameEquals("sortBy")) sortBy = property.Value.GetString();
                else if (property.NameEquals("descending")) descending = property.Value.GetBoolean();
                else if (property.NameEquals("keyword")) keyword = property.Value.GetString();
                else if (property.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined && definition.Columns.Any(c => string.Equals(c.PropertyName, property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var column = definition.Columns.First(c => string.Equals(c.PropertyName, property.Name, StringComparison.OrdinalIgnoreCase));
                    filters.Add(new FilterDefinition(column.PropertyName, property.Value.ValueKind == JsonValueKind.String ? SearchOperator.Contains : SearchOperator.Equals, JsonSerializer.Deserialize(property.Value.GetRawText(), column.PropertyType, JsonOptions)));
                }
            }
        }

        var sort = string.IsNullOrWhiteSpace(sortBy) ? definition.Columns.First(c => !c.IsPrimaryKey).PropertyName : sortBy;
        var options = countOnly ? null : new QueryOptions((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100), Math.Clamp(pageSize, 1, 100));
        var searchFields = definition.Search.Columns.Count > 0 ? definition.Search.Columns.Select(c => c.PropertyName).ToArray() : null;
        return new QueryDefinition(filters, [new SortDefinition(sort, descending ? SortDirection.Descending : SortDirection.Ascending)], options, string.IsNullOrWhiteSpace(keyword) ? null : new SearchOptions(keyword, searchFields));
    }

    private static void AddBooleanFilter(MasterDefinition definition, List<FilterDefinition> filters, string propertyName, bool? value)
    {
        if (value.HasValue && definition.Columns.Any(c => c.PropertyName == propertyName))
        {
            filters.Add(new FilterDefinition(propertyName, SearchOperator.Equals, value.Value));
        }
    }
}
