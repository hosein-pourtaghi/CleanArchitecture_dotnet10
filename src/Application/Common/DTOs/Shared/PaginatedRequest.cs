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
    public List<SortExpression> Sorts { get; set; } = new List<SortExpression>();
    public FilterGroup Filters { get; set; } = new FilterGroup();
    public PaginatedRequest()
    {
        Page = Math.Max(1, Page);
        PageSize = Math.Max(2, Math.Min(PageSize, 1_000));
    }

    //// ===== SAMPLE FILTERING (PROPERTIES) -> for Drived classess from this one =====
    //public Guid? ChecklistId { get; set; }
    //public int? ChecklistVersion { get; set; }
    //public DateTime? FromDate { get; set; }
    //public DateTime? ToDate { get; set; }


}

public class SortExpression
{
    public string Property { get; set; } = string.Empty;
    public bool IsAscending { get; set; } = true;
}

// Represents a group of conditions (either a single condition or a list of sub-groups)
public class FilterGroup
{
    public FilterLogic Logic { get; set; } = FilterLogic.And;
    public List<FilterNode> Nodes { get; set; } = new();
}

// A node can be either a Condition OR another Group (Recursive)
public class FilterNode
{
    public string? Property { get; set; }
    public FilterOperator Operator { get; set; }
    public object? Value { get; set; }
    public FilterGroup? Group { get; set; }
}


public enum FilterOperator
{
    Equal,
    NotEqual,
    Contains,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    StartsWith,
    EndsWith

}

public enum FilterLogic
{
    And,
    Or
}
