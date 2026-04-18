using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Application.Common.DTOs.Shared;
using Application.Common.Interfaces.Core;
using AutoMapper;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Core;

public class BaseRepository<T>(ApplicationDbContext _context, IMapper _mapper) : IBaseRepository<T>
    where T : class
{
    // ==================== CACHING STRATEGIES ====================
    // Cache for PropertyInfo - avoids repeated reflection
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();
    // Cache for compiled expressions - huge performance gain for repeated filters
    private static readonly ConcurrentDictionary<string, Func<object, bool>> CompiledFilterCache = new();
    // Cache for OrderBy methods
    private static readonly ConcurrentDictionary<(Type, Type, bool), MethodInfo> OrderByMethodCache = new();
    // ==================== OPTIMIZED FILTERING ====================

    #region Filtering
    public IQueryable<T> ApplyFiltering(IQueryable<T> query, PaginatedRequest filter)
    {
        if (filter?.Filters == null || !filter.Filters.Nodes.Any())
            return query;

        // Early return if no filters
        if (filter.Filters.Nodes.Count == 0)
            return query;

        // 1. Create the parameter ONCE here
        var parameter = Expression.Parameter(typeof(T), "x");

        // 2. Pass it to the builder
        var expression = BuildExpressionTree(filter.Filters, parameter);

        if (expression == null)
            return query;

        // 3. Use the SAME parameter instance here
        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);

        return query.Where(lambda);

    }
    private Expression? BuildExpressionTree(FilterGroup group, ParameterExpression parameter)
    {
        if (group == null || group?.Nodes == null || group?.Nodes?.Count == 0)
            return null;

        Expression? combinedExpression = null;

        foreach (var node in group.Nodes)
        {
            Expression? currentExpression = null;

            if (node.Group != null)
            {
                // Pass the parameter down recursively
                currentExpression = BuildExpressionTree(node.Group, parameter);
            }
            else if (!string.IsNullOrEmpty(node.Property))
            {
                // Pass the parameter to the condition builder
                currentExpression = BuildConditionExpression(node, parameter);
            }

            if (currentExpression == null)
                continue;

            if (combinedExpression == null)
            {
                combinedExpression = currentExpression;
            }
            else
            {
                combinedExpression = group.Logic == FilterLogic.Or
                    ? Expression.OrElse(combinedExpression, currentExpression)
                    : Expression.AndAlso(combinedExpression, currentExpression);
            }
        }
        return combinedExpression;
    }
    private Expression? BuildConditionExpression(FilterNode node, ParameterExpression parameter)
    {
        // Use cached PropertyInfo
        var property = GetCachedProperty(typeof(T), node.Property!);
        if (property == null)
            return null;

        var propertyAccess = Expression.Property(parameter, property);

        // Convert value with caching
        var convertedValue = ConvertValue(property, node.Value);
        if (convertedValue == null && node.Value != null)
            return null;

        Expression constant;
        if (convertedValue == null)
        {
            constant = Expression.Constant(null, property.PropertyType);
        }
        else
        {
            // Optimize: avoid unnecessary Convert for value types
            if (property.PropertyType == convertedValue.GetType())
            {
                constant = Expression.Constant(convertedValue, property.PropertyType);
            }
            else
            {
                constant = Expression.Constant(convertedValue);
                constant = Expression.Convert(constant, property.PropertyType);
            }
        }

        return node.Operator switch
        {
            FilterOperator.Equal => Expression.Equal(propertyAccess, constant),
            FilterOperator.NotEqual => Expression.NotEqual(propertyAccess, constant),
            FilterOperator.Contains => BuildContainsExpression(propertyAccess, constant),
            FilterOperator.GreaterThan => Expression.GreaterThan(propertyAccess, constant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyAccess, constant),
            FilterOperator.LessThan => Expression.LessThan(propertyAccess, constant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(propertyAccess, constant),
            FilterOperator.StartsWith => Expression.Call(
                propertyAccess,
                "StartsWith",
                Type.EmptyTypes,
                constant),
            FilterOperator.EndsWith => Expression.Call(
                propertyAccess,
                "EndsWith",
                Type.EmptyTypes,
                constant),
            _ => throw new ArgumentOutOfRangeException(nameof(node.Operator))
        };

    }
    private Expression BuildContainsExpression(MemberExpression propertyAccess, Expression constant)
    {
        // Handle both string and collection types
        var containsMethod = typeof(Enumerable).GetMethods()
            .FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            ?.MakeGenericMethod(propertyAccess.Type.GetGenericArguments().FirstOrDefault() ?? typeof(string));

        if (containsMethod != null)
        {
            return Expression.Call(
                containsMethod,
                constant,
                propertyAccess);
        }

        // Fallback to string.Contains
        return Expression.Call(
            propertyAccess,
            "Contains",
            Type.EmptyTypes,
            constant
        );
    }
    private static PropertyInfo? GetCachedProperty(Type type, string propertyName)
    {
        return PropertyCache.GetOrAdd(
            (type, propertyName),
            _ => type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
        );
    }
    private static object? ConvertValue(PropertyInfo property, object? value)
    { 
        if (value == null)
            return null;

        try
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String =>
                        // Special handling for Guids and Dates sent as strings
                        property.PropertyType == typeof(Guid) ? Guid.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTime) ? DateTime.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTime?) && jsonElement.GetString() == null ? null
                        : property.PropertyType == typeof(DateTime?) ? DateTime.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTimeOffset) ? DateTimeOffset.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTimeOffset?) && jsonElement.GetString() == null ? null
                        : property.PropertyType == typeof(DateTimeOffset?) ? DateTimeOffset.Parse(jsonElement.GetString()!)
                        : jsonElement.GetString(),
                    JsonValueKind.Number => property.PropertyType == typeof(int) ? jsonElement.GetInt32() :
                                            property.PropertyType == typeof(long) ? jsonElement.GetInt64() :
                                            property.PropertyType == typeof(decimal) ? jsonElement.GetDecimal() :
                                            property.PropertyType == typeof(double) ? jsonElement.GetDouble() :
                                            property.PropertyType == typeof(float) ? jsonElement.GetSingle() :
                                            jsonElement.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            // Direct conversion for non-JsonElement values
            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                return value;

            return Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        }
        catch
        {
            return null;
        }
    }
    #endregion


    #region Sorting
    public IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter)
    {
        if (filter?.Sorts == null || filter.Sorts.Count == 0)
            return query;

        IOrderedQueryable<T>? orderedQuery = null;
        var isFirst = true;

        foreach (var sort in filter.Sorts)
        {
            var property = GetCachedProperty(typeof(T), sort.Property);
            if (property == null)
                continue;
            // Use cached method
            var method = GetCachedOrderByMethod(typeof(T), property.PropertyType, sort.IsAscending);
            if (method == null)
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            //// FIX: Search by method name and parameter count, then filter for correct types
            //var methodName = sort.IsAscending ? "OrderBy" : "OrderByDescending";
            //// Get the correct method based on sorting direction
            //var method = typeof(Queryable)
            //    .GetMethods()
            //    .FirstOrDefault(m =>
            //        m.Name == methodName &&
            //        m.IsGenericMethodDefinition &&
            //        m.GetParameters().Length == 2 && // (IQueryable source, Expression keySelector)
            //        m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
            //        m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
            //    );

            //if (method == null)
            //{
            //    Console.WriteLine($"Method OrderBy/OrderByDescending not found for type {typeof(T)} and property {property.Name}");
            //    continue;
            //}

            // Make a generic version of the method
            var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);

            if (genericMethod == null)
            {
                Console.WriteLine($"Generic method OrderBy/OrderByDescending not found for type {typeof(T)} and property {property.Name}");
                continue;
            }

            if (isFirst)
            {
                orderedQuery = (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
                isFirst = false;
            }
            else
            {
                orderedQuery = (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { orderedQuery!, lambda })!;
            }

            // Since we used open generics (IQueryable<>), we need to close the generic method
            //var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);

            query = (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda });
        }
        return query;
    }
    private static MethodInfo? GetCachedOrderByMethod(Type entityType, Type propertyType, bool ascending)
    {
        return OrderByMethodCache.GetOrAdd(
            (entityType, propertyType, ascending),
            _ =>
            {
                var methodName = ascending ? "OrderBy" : "OrderByDescending";
                return typeof(Queryable).GetMethods()
                    .FirstOrDefault(m =>
                        m.Name == methodName &&
                        m.IsGenericMethodDefinition &&
                        m.GetParameters().Length == 2);
            }
        );
    }
    #endregion


    public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync([id], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Entity with ID {id} not found");
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await _context.Set<T>().AddRangeAsync(entityList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entityList;
    }
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().UpdateRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResult<TDto>> GetAllAsync<TDto>(PaginatedRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsQueryable();

        // Apply filters (simple + advanced)
        query = ApplyFiltering(query, filter);

        // Apply sorting
        query = ApplySorting(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PaginatedResult<TDto>(
                new List<TDto>(),
                0,
                filter.Page,
                filter.PageSize
            );
        }

        // Optimize skip/take calculations
        var skip = (filter.Page - 1) * filter.PageSize;

        // If requested page is beyond available data, adjust to last page
        if (skip >= totalCount)
        {
            filter.Page = (totalCount / filter.PageSize) + 1;
            skip = (filter.Page - 1) * filter.PageSize;
        }

        // Project to DTO (on query level)
        var dtoQuery = _mapper.ProjectTo<TDto>(query);

        // Apply pagination
        var items = await dtoQuery
                   .Skip(skip)
                   .Take(filter.PageSize)
                   .ToListAsync(cancellationToken);

        return new PaginatedResult<TDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<PaginatedResult<T>> GetAllAsync(PaginatedRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsQueryable();

        // Apply filters (simple + advanced)
        query = ApplyFiltering(query, filter);

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PaginatedResult<T>(
                new List<T>(),
                0,
                filter.Page,
                filter.PageSize
            );
        }

        var skip = (filter.Page - 1) * filter.PageSize;

        if (skip >= totalCount)
        {
            filter.Page = (totalCount / filter.PageSize) + 1;
            skip = (filter.Page - 1) * filter.PageSize;
        }

        var items = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, filter.Page, filter.PageSize);
    }


    #region STREAMING FOR LARGE DATASETS
    /// <summary>
    /// Stream results for very large datasets - memory efficient
    /// </summary>
    public async IAsyncEnumerable<T> StreamAllAsync(
        PaginatedRequest filter,
        //Expression<Func<T, bool>>? additionalFilter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsQueryable();

        query = ApplyFiltering(query, filter);

        //if (additionalFilter != null)
        //    query = query.Where(additionalFilter);

        query = ApplySorting(query, filter);

        // Use AsAsyncEnumerable for streaming
        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Stream DTO results for very large datasets
    /// </summary>
    public async IAsyncEnumerable<TDto> StreamAllAsync<TDto>(
        PaginatedRequest filter, 
        //Expression<Func<T, bool>>? additionalFilter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsQueryable();

        query = ApplyFiltering(query, filter);

        //if (additionalFilter != null)
        //    query = query.Where(additionalFilter);

        query = ApplySorting(query, filter);

        var dtoQuery = _mapper.ProjectTo<TDto>(query);

        await foreach (var item in dtoQuery.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
    #endregion


    //#region Extra
    //// ==================== BULK OPERATIONS ====================

    ///// <summary>
    ///// Bulk insert for high performance
    ///// </summary>
    //public async Task BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    //{
    //    await _context.BulkInsertAsync(entities.ToList(), new BulkConfig
    //    {
    //        BatchSize = 1000,
    //        UseInternalTransaction = false
    //    }, cancellationToken: cancellationToken);
    //}

    ///// <summary>
    ///// Bulk update for high performance
    ///// </summary>
    //public async Task BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    //{
    //    await _context.BulkUpdateAsync(entities.ToList(), new BulkConfig
    //    {
    //        BatchSize = 1000
    //    }, cancellationToken: cancellationToken);
    //}

    ///// <summary>
    ///// Bulk delete for high performance
    ///// </summary>
    //public async Task BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    //{
    //    await _context.Set<T>()
    //        .Where(predicate)
    //        .BatchDeleteAsync(cancellationToken);
    //}

    //// ==================== UTILITY METHODS ====================

    ///// <summary>
    ///// Check if any records exist - optimized
    ///// </summary>
    //public async Task<bool> AnyAsync(
    //    PaginatedRequest filter,
    //    Expression<Func<T, bool>>? additionalFilter = null,
    //    CancellationToken cancellationToken = default)
    //{
    //    var query = _context.Set<T>().AsQueryable();
    //    query = ApplyFiltering(query, filter);

    //    if (additionalFilter != null)
    //        query = query.Where(additionalFilter);

    //    return await query.AnyAsync(cancellationToken);
    //}

    ///// <summary>
    ///// Get count efficiently
    ///// </summary>
    //public async Task<int> CountAsync(
    //    PaginatedRequest filter,
    //    Expression<Func<T, bool>>? additionalFilter = null,
    //    CancellationToken cancellationToken = default)
    //{
    //    var query = _context.Set<T>().AsQueryable();
    //    query = ApplyFiltering(query, filter);

    //    if (additionalFilter != null)
    //        query = query.Where(additionalFilter);

    //    return await query.CountAsync(cancellationToken);
    //}

    ///// <summary>
    ///// Execute raw SQL for complex queries
    ///// </summary>
    //public async Task<List<T>> ExecuteRawQueryAsync(
    //    string sql,
    //    CancellationToken cancellationToken = default,
    //    params object[] parameters)
    //{
    //    return await _context.Set<T>()
    //        .FromSqlRaw(sql, parameters)
    //        .ToListAsync(cancellationToken);
    //}

    ///// <summary>
    ///// Clear all caches (useful for testing)
    ///// </summary>
    //public static void ClearCaches()
    //{
    //    PropertyCache.Clear();
    //    CompiledFilterCache.Clear();
    //    OrderByMethodCache.Clear();
    //}
    //#endregion


}
