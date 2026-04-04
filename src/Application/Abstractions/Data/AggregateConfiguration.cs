using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
 
namespace Application.Abstractions.Data;

public class AggregateConfiguration<TEntity> where TEntity : class
{
    /// <summary>
    /// Custom function to get primary key value from entity
    /// </summary>
    public Func<TEntity, object>? GetKeyFunc { get; set; }

    /// <summary>
    /// Properties to exclude from update (like CreatedDate, etc.)
    /// </summary>
    public List<string> ExcludedProperties { get; set; } = new();

    /// <summary>
    /// List of navigation properties to manage (if empty, auto-detects all)
    /// </summary>
    public List<string> NavigationPropertiesToInclude { get; set; } = new();

    /// <summary>
    /// Custom logic to run before saving
    /// </summary>
    public Action<TEntity, TEntity, DbContext>? PreSaveAction { get; set; }

    /// <summary>
    /// Custom logic to run after saving
    /// </summary>
    public Action<TEntity, TEntity, DbContext>? PostSaveAction { get; set; }

    /// <summary>
    /// Whether to load all nested entities (default: true)
    /// </summary>
    public bool LoadAllNested { get; set; } = true;

    /// <summary>
    /// Maximum depth for nested loading (default: 3)
    /// </summary>
    public int MaxNestingDepth { get; set; } = 3;
}
