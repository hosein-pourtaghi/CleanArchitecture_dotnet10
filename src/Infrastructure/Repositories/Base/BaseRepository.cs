using System.Linq.Expressions;
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
        // Apply all conditions
        foreach (var condition in filter.FilterConditions)
        {
            var property = typeof(T).GetProperty(condition.Property);
            if (property == null)
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var constant = Expression.Constant(condition.Value);

            Expression conditionExpression = condition.Operator switch
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

            var lambda = Expression.Lambda<Func<T, bool>>(conditionExpression, parameter);
            query = condition.Logic == FilterLogic.And
                ? query.Where(lambda)
                : query.Where(lambda).AsQueryable();
        }
        return query;
    }

    public IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter)
    {
        foreach (var sort in filter.SortExpressions)
        {
            var property = typeof(T).GetProperty(sort.Property);
            if (property == null)
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderBy = Expression.Lambda(propertyAccess, parameter);
            var method = sort.IsAscending ?
                typeof(Queryable).GetMethod("OrderBy") :
                typeof(Queryable).GetMethod("OrderByDescending");

            var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);
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
