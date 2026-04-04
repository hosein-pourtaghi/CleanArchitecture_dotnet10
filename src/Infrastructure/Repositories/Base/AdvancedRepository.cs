using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Application.Abstractions.Data;
using Application.Abstractions.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Repositories.Base;

/// <summary>
/// Advanced generic repository for aggregate roots with full CRUD support
/// </summary>
public abstract class AdvancedRepository<TEntity>(ApplicationDbContext _context) : IAdvancedRepository<TEntity>
    where TEntity : class
{


    #region Basic CRUD

    public virtual async Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Querying

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _context.Set<TEntity>().CountAsync(cancellationToken)
            : await _context.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Include/Load Related Data

    public virtual async Task<TEntity?> GetByIdWithIncludesAsync<TKey>(
        TKey id,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id));
    }

    public virtual async Task<List<TEntity>> GetAllWithIncludesAsync(
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    #endregion

    #region Aggregate Update (Generic)

    /// <summary>
    /// Generic method to update aggregate root with all nested entities
    /// </summary>
    public virtual async Task<TEntity> UpdateAggregateAsync<TKey>(
        TKey id,
        TEntity newEntity,
        CancellationToken cancellationToken = default)
    {
        var configuration = CreateDefaultConfiguration();
        return await UpdateAggregateAsync(id, newEntity, configuration, cancellationToken);
    }

    /// <summary>
    /// Generic method with custom configuration
    /// </summary>
    public virtual async Task<TEntity> UpdateAggregateAsync<TKey>(
        TKey id,
        TEntity newEntity,
        AggregateConfiguration<TEntity> configuration,
        CancellationToken cancellationToken = default)
    {
        // 1. Load existing aggregate with all nested entities
        var existingEntity = await LoadAggregateWithAllNestedAsync<TKey>(id, configuration, cancellationToken);

        if (existingEntity == null)
            throw new KeyNotFoundException($"Entity with ID {id} not found");

        // 2. Get key property info
        var keyProperty = GetKeyProperty(typeof(TEntity));
        var keyValue = keyProperty.GetValue(existingEntity);

        // 3. Run pre-save action if configured
        configuration.PreSaveAction?.Invoke(existingEntity, newEntity, _context);

        // 4. Update root entity properties
        UpdateEntityProperties(existingEntity, newEntity, configuration.ExcludedProperties);

        // 5. Mark root as Modified
        _context.Entry(existingEntity).State = EntityState.Modified;

        // 6. Process all navigation properties
        await ProcessAllNavigationPropertiesAsync(
            existingEntity,
            newEntity,
            keyValue,
            0,
            configuration,
            cancellationToken);

        // 7. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // 8. Run post-save action if configured
        configuration.PostSaveAction?.Invoke(existingEntity, newEntity, _context);

        // 9. Return updated entity
        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve updated entity");
    }

    #endregion

    #region Batch Operations

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().UpdateRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Protected Virtual Methods

    /// <summary>
    /// Override to provide custom configuration for specific aggregates
    /// </summary>
    protected virtual AggregateConfiguration<TEntity> CreateDefaultConfiguration()
    {
        return new AggregateConfiguration<TEntity>
        {
            ExcludedProperties = new List<string>
            {
                "Id",
                "CreatedDate",
                "CreatedBy",
                "RowVersion"
            }
        };
    }

    protected virtual async Task<TEntity?> LoadAggregateWithAllNestedAsync<TKey>(
        TKey id,
        AggregateConfiguration<TEntity> configuration,
        CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity);
        var navigationProperties = GetCollectionNavigationProperties(entityType);

        IQueryable<TEntity> query = _context.Set<TEntity>().Where(e => GetKeyValue(e)!.Equals(id));

        if (configuration.LoadAllNested)
        {
            // Include all collection navigation properties up to max depth
            foreach (var navProp in navigationProperties)
            {
                query = query.Include(navProp.Name);

                // Include nested collections
                var nestedNavProps = GetCollectionNavigationProperties(navProp.PropertyType);
                foreach (var nestedNav in nestedNavProps.Take(configuration.MaxNestingDepth - 1))
                {
                    query = query.Include($"{navProp.Name}.{nestedNav.Name}");
                }
            }
        }
        else if (configuration.NavigationPropertiesToInclude.Any())
        {
            foreach (var navName in configuration.NavigationPropertiesToInclude)
            {
                query = query.Include(navName);
            }
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    private async Task ProcessAllNavigationPropertiesAsync(
        object parentEntity,
        object newParentEntity,
        object? parentKeyValue,
        int depth,
        AggregateConfiguration<TEntity> configuration,
        CancellationToken cancellationToken)
    {
        if (depth > configuration.MaxNestingDepth)
            return;

        var navProperties = GetCollectionNavigationProperties(parentEntity.GetType());

        foreach (var navProp in navProperties)
        {
            await ProcessNavigationPropertyAsync(
                parentEntity,
                newParentEntity,
                navProp,
                parentKeyValue,
                depth,
                configuration,
                cancellationToken);
        }
    }

    private async Task ProcessNavigationPropertyAsync(
        object parentEntity,
        object newParentEntity,
        PropertyInfo navProperty,
        object? parentKeyValue,
        int depth,
        AggregateConfiguration<TEntity> configuration,
        CancellationToken cancellationToken)
    {
        var oldCollection = navProperty.GetValue(parentEntity) as IEnumerable<object>;
        var newCollection = navProperty.GetValue(newParentEntity) as IEnumerable<object>;

        var oldList = oldCollection?.ToList() ?? new List<object>();
        var newList = newCollection?.ToList() ?? new List<object>();

        if (!oldList.Any() && !newList.Any())
            return;

        var keyProperty = GetKeyProperty(navProperty.PropertyType);
        if (keyProperty == null)
            return;

        var oldIds = oldList.Select(e => keyProperty.GetValue(e)).ToHashSet();
        var newIds = newList.Select(e => keyProperty.GetValue(e)).ToHashSet();

        // 1. Delete: items in old but not in new
        var toDelete = oldList.Where(e => !newIds.Contains(keyProperty.GetValue(e))).ToList();
        foreach (var item in toDelete)
        {
            await DeleteEntityWithAllNestedAsync(item, depth + 1, configuration);
        }

        // 2. Add: items in new but not in old
        var toAdd = newList.Where(e => !oldIds.Contains(keyProperty.GetValue(e))).ToList();
        foreach (var item in toAdd)
        {
            // Set foreign key
            SetForeignKey(parentEntity, item, navProperty.Name, parentKeyValue);

            // Add to collection
            AddToCollection(parentEntity, navProperty.Name, item);

            // Mark as Added
            _context.Entry(item).State = EntityState.Added;

            // Process children
            await ProcessAllNavigationPropertiesAsync(item, item, keyProperty.GetValue(item), depth + 1, configuration, cancellationToken);
        }

        // 3. Update: items in both
        var toUpdate = newList.Where(e => oldIds.Contains(keyProperty.GetValue(e))).ToList();
        foreach (var newItem in toUpdate)
        {
            var oldItem = oldList.First(e => keyProperty.GetValue(e)!.Equals(keyProperty.GetValue(newItem)));

            // Update properties
            UpdateEntityProperties(oldItem, newItem, configuration.ExcludedProperties);
            _context.Entry(oldItem).State = EntityState.Modified;

            // Process children
            await ProcessAllNavigationPropertiesAsync(oldItem, newItem, keyProperty.GetValue(oldItem), depth + 1, configuration, cancellationToken);
        }
    }

    private async Task DeleteEntityWithAllNestedAsync(object entity, int depth, AggregateConfiguration<TEntity> configuration)
    {
        if (depth > configuration.MaxNestingDepth)
            return;

        // Delete all children first
        var navProperties = GetCollectionNavigationProperties(entity.GetType());

        foreach (var navProp in navProperties)
        {
            var children = navProp.GetValue(entity) as IEnumerable<object>;
            if (children != null)
            {
                foreach (var child in children.ToList())
                {
                    await DeleteEntityWithAllNestedAsync(child, depth + 1, configuration);
                }
            }
        }

        // Delete the entity itself
        _context.Entry(entity).State = EntityState.Deleted;
    }

    private void UpdateEntityProperties(object target, object source, List<string> excludedProperties)
    {
        var properties = source.GetType().GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !excludedProperties.Contains(p.Name))
            .Where(p => !IsNavigationProperty(p));

        foreach (var prop in properties)
        {
            var value = prop.GetValue(source);
            prop.SetValue(target, value);
        }
    }

    private void SetForeignKey(object parent, object child, string navigationPropertyName, object? keyValue)
    {
        if (keyValue == null)
            return;

        var parentType = parent.GetType();

        // Try to find foreign key property
        var fkName = parentType.Name + "Id";
        var fkProperty = child.GetType().GetProperty(fkName, BindingFlags.Public | BindingFlags.IgnoreCase);

        if (fkProperty != null && fkProperty.CanWrite)
        {
            fkProperty.SetValue(child, keyValue);
            return;
        }

        // Try singularized name
        var singularName = navigationPropertyName.TrimEnd('s');
        fkName = singularName + "Id";
        fkProperty = child.GetType().GetProperty(fkName, BindingFlags.Public | BindingFlags.IgnoreCase);

        if (fkProperty != null && fkProperty.CanWrite)
        {
            fkProperty.SetValue(child, keyValue);
        }
    }

    private void AddToCollection(object parent, string propertyName, object item)
    {
        var collection = parent.GetType().GetProperty(propertyName);
        if (collection == null)
            return;

        var collectionValue = collection.GetValue(parent);
        if (collectionValue == null)
            return;

        var addMethod = collectionValue.GetType().GetMethod("Add");
        addMethod?.Invoke(collectionValue, new[] { item });
    }

    #endregion

    #region Reflection Helpers

    private static PropertyInfo? GetKeyProperty(Type entityType)
    {
        // Try to find 'Id' property
        var idProperty = entityType.GetProperties()
            .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

        if (idProperty != null)
            return idProperty;

        // Try to find property with [Key] attribute
        return entityType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
    }

    private static object? GetKeyValue(object entity)
    {
        return GetKeyProperty(entity.GetType())?.GetValue(entity);
    }

    private static List<PropertyInfo> GetCollectionNavigationProperties(Type entityType)
    {
        return entityType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                       typeof(ICollection<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
            .ToList();
    }

    private static bool IsNavigationProperty(PropertyInfo property)
    {
        // Check if it's a collection
        if (property.PropertyType.IsGenericType &&
            typeof(ICollection<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition()))
            return true;

        // Check if it's a reference navigation (has corresponding foreign key)
        var fkName = property.Name + "Id";
        return property.DeclaringType?.GetProperty(fkName, BindingFlags.Public | BindingFlags.IgnoreCase) != null;
    }

    #endregion
}



