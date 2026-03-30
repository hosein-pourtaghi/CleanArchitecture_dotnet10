using System.Linq.Expressions;
using System.Text.Json;
using Application.Abstractions.Interfaces;
using Application.Common.DTOs.Shared;
using AutoMapper;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Base;

public class BaseRepository<T>(ApplicationDbContext _context, IMapper _mapper) : IBaseRepository<T>
    where T : class
{ 
    public IQueryable<T> ApplyFiltering(IQueryable<T> query, PaginatedRequest filter)
    {
        if (filter?.Filters == null || !filter.Filters.Nodes.Any())
            return query;
        // 1. Create the parameter ONCE here
        var parameter = Expression.Parameter(typeof(T), "x");

        // 2. Pass it to the builder
        var expression = BuildExpressionTree(filter.Filters, typeof(T), parameter);
        if (expression != null)
        {
            // 3. Use the SAME parameter instance here
            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
            query = query.Where(lambda);
        }
        return query;

    }

    private Expression BuildExpressionTree(FilterGroup group, Type type, ParameterExpression parameter)
    {
        if (group == null || !group.Nodes.Any())
            return null;

        Expression? combinedExpression = null;

        foreach (var node in group.Nodes)
        {
            Expression? currentExpression = null;

            if (node.Group != null)
            {
                // Pass the parameter down recursively
                currentExpression = BuildExpressionTree(node.Group, type, parameter);
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

    // Updated signature to accept 'parameter'
    private Expression BuildConditionExpression(FilterNode node, ParameterExpression parameter)
    {
        var property = typeof(T).GetProperty(node.Property!);
        if (property == null)
            return null;

        var propertyAccess = Expression.Property(parameter, property);
        object convertedValue;

        try
        {
            if (node.Value is JsonElement jsonElement)
            {
                convertedValue = jsonElement.ValueKind switch
                {
                    JsonValueKind.String =>
                        // Special handling for Guids and Dates sent as strings
                        property.PropertyType == typeof(Guid) ? Guid.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTime) ? DateTime.Parse(jsonElement.GetString()!)
                        : property.PropertyType == typeof(DateTime?) && jsonElement.GetString() == null ? null
                        : property.PropertyType == typeof(DateTime?) ? DateTime.Parse(jsonElement.GetString()!)
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
            else
            {
                convertedValue = Convert.ChangeType(node.Value, property.PropertyType);
            }
        }
        catch
        {
            return null;
        }

        Expression constant;
        if (convertedValue == null)
        {
            constant = Expression.Constant(null, property.PropertyType);
        }
        else
        {
            var constantValue = Expression.Constant(convertedValue);
            constant = Expression.Convert(constantValue, property.PropertyType);
        }

        return node.Operator switch
        {
            FilterOperator.Equal => Expression.Equal(propertyAccess, constant),
            FilterOperator.Contains => Expression.Call(
                propertyAccess,
                "Contains",
                Type.EmptyTypes,
                constant
            ),
            FilterOperator.GreaterThan => Expression.GreaterThan(propertyAccess, constant),
            FilterOperator.LessThan => Expression.LessThan(propertyAccess, constant),
            _ => throw new ArgumentOutOfRangeException()
        };
    }








    public IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter)
    {
        //if (filter?.SortExpressions == null)
        //    return query;

        foreach (var sort in filter.SortExpressions)
        {
            var property = typeof(T).GetProperty(sort.Property);
            if (property == null)
                continue;
            // Create the parameter expression (x => x.SomeProperty)
            var parameter = Expression.Parameter(typeof(T), "x");
            // Access the property on the parameter (x.SomeProperty)
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            // Lambda expression to order by the property 
            var orderBy = Expression.Lambda(propertyAccess, parameter);
            // FIX: Search by method name and parameter count, then filter for correct types
            var methodName = sort.IsAscending ? "OrderBy" : "OrderByDescending";
            // Get the correct method based on sorting direction
            var method = typeof(Queryable)
                .GetMethods()
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 2 && // (IQueryable source, Expression keySelector)
                    m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
                    m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
                );

            if (method == null)
            {
                Console.WriteLine($"Method OrderBy/OrderByDescending not found for type {typeof(T)} and property {property.Name}");
                continue;
            }

            // Make a generic version of the method
            var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);

            if (genericMethod == null)
            {
                Console.WriteLine($"Generic method OrderBy/OrderByDescending not found for type {typeof(T)} and property {property.Name}");
                continue;
            }


            // Since we used open generics (IQueryable<>), we need to close the generic method
            //var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);

            query = (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, orderBy });
        }
        return query;
    }

    public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<T>().FindAsync([id], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Entity with ID {id} not found");
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }


    public async Task<PaginatedResult<TDto>> GetAllAsync<TDto>(
      PaginatedRequest filter,
      Expression<Func<T, bool>>? additionalFilter = null)
    {
        var query = _context.Set<T>().AsQueryable();

        // Apply filters (simple + advanced)
        query = ApplyFiltering(query, filter);

        // Apply additional filter (AND)
        if (additionalFilter != null)
            query = query.Where(additionalFilter);

        // Apply sorting
        query = ApplySorting(query, filter);

        // Project to DTO (on query level)
        var dtoQuery = _mapper.ProjectTo<TDto>(query);

        // Apply pagination
        var totalCount = await dtoQuery.CountAsync();
        var dtoItems = await dtoQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<TDto>(dtoItems, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<PaginatedResult<T>> GetAllAsync(
      PaginatedRequest filter,
      Expression<Func<T, bool>>? additionalFilter = null)
    {
        var query = _context.Set<T>().AsQueryable();

        // Apply filters (simple + advanced)
        query = ApplyFiltering(query, filter);

        // Apply additional filter (AND)
        if (additionalFilter != null)
            query = query.Where(additionalFilter);

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var totalCount = await query.CountAsync();
        var paginatedQuery = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<T>(paginatedQuery, totalCount, filter.Page, filter.PageSize);
    }

}
