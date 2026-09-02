using System.Collections;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Search;

namespace Orion.Framework.Crud;

/// <summary>
/// Executes metadata-driven CRUD operations for registered masters
/// resolved at runtime.
/// </summary>
public interface IGenericMasterCrudService
{
    Task<object> CreateAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken);

    Task<object?> GetAsync(
        string master,
        object key,
        CancellationToken cancellationToken);

    Task<object> UpdateAsync(
        string master,
        object key,
        JsonElement payload,
        CancellationToken cancellationToken);

    Task<int> DeleteAsync(
        string master,
        object key,
        CancellationToken cancellationToken);

    Task<int> SoftDeleteAsync(
        string master,
        object key,
        CancellationToken cancellationToken);

    Task<int> RestoreAsync(
        string master,
        object key,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<object>> SearchAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken);

    Task<long> CountAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        string master,
        object key,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<object>> DropdownAsync(
        string master,
        bool activeOnly,
        string? searchText,
        int top,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default metadata-driven master CRUD service.
/// </summary>
public sealed class GenericMasterCrudService(
    IMasterRegistry registry,
    IServiceProvider services)
    : IGenericMasterCrudService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    /// <inheritdoc />
    public async Task<object> CreateAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var entity = Deserialize(
            definition,
            payload);

        var result = await InvokeAsync<object>(
            definition,
            nameof(ICrudService<object>.CreateAsync),
            [entity, cancellationToken])
            .ConfigureAwait(false);

        return result ?? entity;
    }

    /// <inheritdoc />
    public Task<object?> GetAsync(
        string master,
        object key,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var convertedKey =
            ConvertKey(
                definition,
                key);

        return InvokeAsync<object?>(
            definition,
            nameof(ICrudService<object>.GetByIdAsync),
            [convertedKey, cancellationToken]);
    }

    /// <inheritdoc />
    public async Task<object> UpdateAsync(
        string master,
        object key,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var entity = Deserialize(
            definition,
            payload);

        SetPrimaryKey(
            definition,
            entity,
            key);

        var result = await InvokeAsync<object>(
            definition,
            nameof(ICrudService<object>.UpdateAsync),
            [entity, cancellationToken])
            .ConfigureAwait(false);

        return result ?? entity;
    }

    /// <inheritdoc />
    public Task<int> DeleteAsync(
        string master,
        object key,
        CancellationToken cancellationToken)
    {
        return RowsAsync(
            master,
            nameof(ICrudService<object>.DeleteAsync),
            key,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> SoftDeleteAsync(
        string master,
        object key,
        CancellationToken cancellationToken)
    {
        return RowsAsync(
            master,
            nameof(ICrudService<object>.SoftDeleteAsync),
            key,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> RestoreAsync(
        string master,
        object key,
        CancellationToken cancellationToken)
    {
        return RowsAsync(
            master,
            nameof(ICrudService<object>.RestoreAsync),
            key,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<object>> SearchAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var query = BuildQuery(
            definition,
            payload,
            false);

        var rows = await InvokeAsync<object>(
            definition,
            nameof(ICrudService<object>.GetManyAsync),
            [query, cancellationToken])
            .ConfigureAwait(false);

        return ConvertToObjectList(rows);
    }

    /// <inheritdoc />
    public Task<long> CountAsync(
        string master,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var query = BuildQuery(
            definition,
            payload,
            true);

        return InvokeAsync<long>(
            definition,
            nameof(ICrudService<object>.CountAsync),
            [query, cancellationToken]);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string master,
        object key,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var convertedKey =
            ConvertKey(
                definition,
                key);

        return InvokeAsync<bool>(
            definition,
            nameof(ICrudService<object>.ExistsAsync),
            [convertedKey, cancellationToken]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<object>> DropdownAsync(
        string master,
        bool activeOnly,
        string? searchText,
        int top,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(master);

        var filters =
            new List<FilterDefinition>();

        AddBooleanFilter(
            definition,
            filters,
            "IsActive",
            activeOnly ? true : null);

        AddBooleanFilter(
            definition,
            filters,
            "IsDeleted",
            false);

        var sorts = BuildDropdownSorts(
            definition);

        var searchFields =
            definition.Columns
                .Where(column =>
                    column.IsDropdownColumn ||
                    column.IsSearchable)
                .Select(column =>
                    column.PropertyName)
                .ToArray();

        var search =
            string.IsNullOrWhiteSpace(searchText)
                ? null
                : new SearchOptions(
                    searchText,
                    searchFields);

        var options =
            new QueryOptions(
                Top: Math.Clamp(
                    top,
                    1,
                    100));

        var query =
            new QueryDefinition(
                filters,
                sorts,
                options,
                search);

        var rows = await InvokeAsync<object>(
            definition,
            nameof(ICrudService<object>.GetManyAsync),
            [query, cancellationToken])
            .ConfigureAwait(false);

        return ConvertToObjectList(rows);
    }

    private async Task<int> RowsAsync(
        string master,
        string method,
        object key,
        CancellationToken cancellationToken)
    {
        var definition =
            GetDefinition(master);

        var convertedKey =
            ConvertKey(
                definition,
                key);

        return await InvokeAsync<int>(
            definition,
            method,
            [convertedKey, cancellationToken])
            .ConfigureAwait(false);
    }

    private MasterDefinition GetDefinition(
        string master)
    {
        return registry.Find(master)
            ?? throw new KeyNotFoundException(
                $"Master '{master}' is not registered.");
    }

    private static object Deserialize(
        MasterDefinition definition,
        JsonElement payload)
    {
        return JsonSerializer.Deserialize(
                   payload.GetRawText(),
                   definition.EntityType,
                   JsonOptions)
               ?? throw new InvalidOperationException(
                   $"Invalid {definition.EntityName} payload.");
    }

    private static object ConvertKey(
        MasterDefinition definition,
        object key)
    {
        var keyColumn =
            GetPrimaryKey(definition);

        var targetType =
            Nullable.GetUnderlyingType(
                keyColumn.PropertyType)
            ?? keyColumn.PropertyType;

        return Convert.ChangeType(
            key,
            targetType);
    }

    private static void SetPrimaryKey(
        MasterDefinition definition,
        object entity,
        object key)
    {
        var keyColumn =
            GetPrimaryKey(definition);

        var property =
            definition.EntityType.GetProperty(
                keyColumn.PropertyName)
            ?? throw new InvalidOperationException(
                $"Primary key property '{keyColumn.PropertyName}' " +
                $"was not found on '{definition.EntityType.Name}'.");

        var convertedKey =
            ConvertKey(
                definition,
                key);

        property.SetValue(
            entity,
            convertedKey);
    }

    private static ColumnDefinition GetPrimaryKey(
        MasterDefinition definition)
    {
        return definition.Columns
                   .FirstOrDefault(
                       column => column.IsPrimaryKey)
               ?? throw new InvalidOperationException(
                   $"Master '{definition.EntityName}' " +
                   "has no primary key.");
    }

    private async Task<T?> InvokeAsync<T>(
        MasterDefinition definition,
        string methodName,
        object?[] args)
    {
        var serviceType =
            typeof(ICrudService<>)
                .MakeGenericType(
                    definition.EntityType);

        var service =
            services.GetRequiredService(
                serviceType);

        var method =
            serviceType.GetMethod(
                methodName,
                BindingFlags.Instance |
                BindingFlags.Public)
            ?? throw new MissingMethodException(
                serviceType.Name,
                methodName);

        var task =
            (Task)method.Invoke(
                service,
                args)!;

        await task
            .ConfigureAwait(false);

        if (!task.GetType().IsGenericType)
        {
            return default;
        }

        var resultProperty =
            task.GetType().GetProperty(
                nameof(Task<object>.Result));

        return resultProperty is null
            ? default
            : (T?)resultProperty.GetValue(task);
    }

    private static QueryDefinition BuildQuery(
        MasterDefinition definition,
        JsonElement payload,
        bool countOnly)
    {
        var filters =
            new List<FilterDefinition>();

        var page = 1;
        var pageSize = 20;
        string? sortBy = null;
        var descending = false;
        string? keyword = null;

        if (payload.ValueKind ==
            JsonValueKind.Object)
        {
            foreach (var property
                     in payload.EnumerateObject())
            {
                ProcessQueryProperty(
                    definition,
                    property,
                    filters,
                    ref page,
                    ref pageSize,
                    ref sortBy,
                    ref descending,
                    ref keyword);
            }
        }

        var sort =
            string.IsNullOrWhiteSpace(sortBy)
                ? definition.Columns
                    .First(column => !column.IsPrimaryKey)
                    .PropertyName
                : sortBy;

        var options =
            countOnly
                ? null
                : new QueryOptions(
                    Offset:
                        (Math.Max(1, page) - 1) *
                        Math.Clamp(
                            pageSize,
                            1,
                            100),
                    PageSize:
                        Math.Clamp(
                            pageSize,
                            1,
                            100));

        var searchFields =
            definition.Search.Columns.Count > 0
                ? definition.Search.Columns
                    .Select(column =>
                        column.PropertyName)
                    .ToArray()
                : null;

        var search =
            string.IsNullOrWhiteSpace(keyword)
                ? null
                : new SearchOptions(
                    keyword,
                    searchFields);

        var direction =
            descending
                ? SortDirection.Descending
                : SortDirection.Ascending;

        return new QueryDefinition(
            filters,
            [new SortDefinition(
                sort,
                direction)],
            options,
            search);
    }

    private static void ProcessQueryProperty(
        MasterDefinition definition,
        JsonProperty property,
        List<FilterDefinition> filters,
        ref int page,
        ref int pageSize,
        ref string? sortBy,
        ref bool descending,
        ref string? keyword)
    {
        if (property.NameEquals("page"))
        {
            page =
                property.Value.GetInt32();

            return;
        }

        if (property.NameEquals("pageSize"))
        {
            pageSize =
                property.Value.GetInt32();

            return;
        }

        if (property.NameEquals("sortBy"))
        {
            sortBy =
                property.Value.GetString();

            return;
        }

        if (property.NameEquals("descending"))
        {
            descending =
                property.Value.GetBoolean();

            return;
        }

        if (property.NameEquals("keyword"))
        {
            keyword =
                property.Value.GetString();

            return;
        }

        if (property.Value.ValueKind is
            JsonValueKind.Null or
            JsonValueKind.Undefined)
        {
            return;
        }

        var column =
            definition.Columns.FirstOrDefault(
                candidate =>
                    string.Equals(
                        candidate.PropertyName,
                        property.Name,
                        StringComparison.OrdinalIgnoreCase));

        if (column is null)
        {
            return;
        }

        var operation =
            property.Value.ValueKind ==
            JsonValueKind.String
                ? SearchOperator.Contains
                : SearchOperator.Equals;

        var value =
            JsonSerializer.Deserialize(
                property.Value.GetRawText(),
                column.PropertyType,
                JsonOptions);

        filters.Add(
            new FilterDefinition(
                column.PropertyName,
                operation,
                value));
    }

    private static IReadOnlyList<SortDefinition> BuildDropdownSorts(
        MasterDefinition definition)
    {
        var dropdownColumns =
            definition.Columns
                .Where(column =>
                    column.IsDropdownColumn)
                .Select(column =>
                    new SortDefinition(
                        column.PropertyName))
                .ToArray();

        if (dropdownColumns.Length > 0)
        {
            return dropdownColumns;
        }

        var fallbackColumn =
            definition.Columns.First(
                column => !column.IsPrimaryKey);

        return
        [
            new SortDefinition(
                fallbackColumn.PropertyName)
        ];
    }

    private static IReadOnlyList<object> ConvertToObjectList(
        object? rows)
    {
        if (rows is not IEnumerable enumerable)
        {
            return Array.Empty<object>();
        }

        return enumerable
            .Cast<object>()
            .ToArray();
    }

    private static void AddBooleanFilter(
        MasterDefinition definition,
        List<FilterDefinition> filters,
        string propertyName,
        bool? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        var columnExists =
            definition.Columns.Any(
                column => column.PropertyName == propertyName);

        if (!columnExists)
        {
            return;
        }

        filters.Add(
            new FilterDefinition(
                propertyName,
                SearchOperator.Equals,
                value.Value));
    }
}
