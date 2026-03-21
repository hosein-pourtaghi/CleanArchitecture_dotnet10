using System;
using System.Linq.Expressions;
using Application.Common.DTOs.Shared;
using AutoMapper;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

//public abstract class BaseRepository<T> where T : class
//{
//    protected readonly ApplicationDbContext _context;

//    protected BaseRepository(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public virtual async Task<PaginatedResult<T>> GetAllAsync(BaseFilter filter)
//    {
//        var query = _context.Set<T>().AsQueryable();

//        // Apply sorting (handles multiple sort expressions)
//        query = ApplySorting(query, filter);

//        // Apply entity-specific filters
//        query = ApplyEntityFilters(query, filter);

//        // Apply pagination
//        var totalCount = await query.CountAsync();
//        var items = await query
//            .Skip((filter.Page - 1) * filter.PageSize)
//            .Take(filter.PageSize)
//            .ToListAsync();

//        return new PaginatedResult<T>(items, totalCount, filter.Page, filter.PageSize);
//    }

//    protected virtual IQueryable<T> ApplyEntityFilters(IQueryable<T> query, BaseFilter filter)
//    {
//        return query;
//    }

//    protected virtual IQueryable<T> ApplySorting(IQueryable<T> query, BaseFilter filter)
//    {
//        foreach (var sort in filter.SortExpressions)
//        {
//            var property = typeof(T).GetProperty(sort.Property);
//            if (property == null)
//                continue;

//            var parameter = Expression.Parameter(typeof(T), "x");
//            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
//            var orderBy = Expression.Lambda(propertyAccess, parameter);
//            var method = sort.IsAscending ?
//                typeof(Queryable).GetMethod("OrderBy") :
//                typeof(Queryable).GetMethod("OrderByDescending");

//            var genericMethod = method.MakeGenericMethod(typeof(T), property.PropertyType);
//            query = (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, orderBy });
//        }
//        return query;
//    }
//}


public class Repository<T>(ApplicationDbContext _context, IMapper _mapper) where T : class
{ 
    public IQueryable<T> ApplyFilter(IQueryable<T> query, Filter filter)
    {
        // Convert simple properties to conditions
        if (filter.ChecklistId.HasValue)
            filter.And("ChecklistId", filter.ChecklistId.Value);

        if (filter.ChecklistVersion.HasValue)
            filter.And("ChecklistVersion", filter.ChecklistVersion.Value);

        if (filter.FromDate.HasValue)
            filter.And("AssessmentDate", filter.FromDate.Value, FilterOperator.GreaterThan);

        if (filter.ToDate.HasValue)
            filter.Or("AssessmentDate", filter.ToDate.Value.AddDays(1).AddTicks(-1), FilterOperator.LessThan);

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

    public IQueryable<T> ApplySorting(IQueryable<T> query, Filter filter)
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

    public async Task<PaginatedResult<TDto>> GetAllAsync<TDto>(
        Filter filter,
        Expression<Func<T, bool>>? additionalFilter = null)
    {
        var query = _context.Set<T>().AsQueryable();

        // Apply filters (simple + advanced)
        query = ApplyFilter(query, filter);

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
}
