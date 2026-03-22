namespace Application.Common.DTOs.Shared;

/// <summary>
/// base request for rilter/sort/pagination 
/// implement this for spesific usecases of filters
/// </summary>
/// PaginatedResult
public class PaginatedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public List<SortExpression> SortExpressions { get; set; } = new List<SortExpression>();
    public List<FilterCondition> FilterConditions { get; set; } = new List<FilterCondition>();

    public PaginatedRequest()
    {
        Validate();
    }

    public void Validate()
    {
        Page = Math.Max(1, Page);
        PageSize = Math.Max(2, Math.Min(PageSize, 100_000));
    }

    // ===== SIMPLE FILTERING (PROPERTIES) =====
    public Guid? ChecklistId { get; set; }
    public int? ChecklistVersion { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // ===== ADVANCED FILTERING (CONDITIONS) =====
    public PaginatedRequest And(string property, object value, FilterOperator op = FilterOperator.Equal)
    {
        FilterConditions.Add(new FilterCondition
        {
            Property = property,
            Value = value,
            Operator = op,
            Logic = FilterLogic.And
        });
        return this;
    }

    public PaginatedRequest Or(string property, object value, FilterOperator op = FilterOperator.Equal)
    {
        FilterConditions.Add(new FilterCondition
        {
            Property = property,
            Value = value,
            Operator = op,
            Logic = FilterLogic.Or
        });
        return this;
    }
}

public class SortExpression
{
    public string Property { get; set; }
    public bool IsAscending { get; set; } = true;
}

public class FilterCondition
{
    public string Property { get; set; }
    public object Value { get; set; }
    public FilterOperator Operator { get; set; } = FilterOperator.Equal;
    public FilterLogic Logic { get; set; } = FilterLogic.And;
}

public enum FilterOperator
{
    Equal,
    Contains,
    GreaterThan,
    LessThan,
    In
}

public enum FilterLogic
{
    And,
    Or
}








// Application/Checklists/ChecklistFilter.cs
//public class ChecklistFilter : BaseFilter
//{
//    public bool? IsActive { get; set; }
//    public string? Title { get; set; }
//}
