using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Models.Binders;


public class GenericFilterModelBinder : IModelBinder
{
    private readonly JsonSerializerOptions _jsonOptions;

    public GenericFilterModelBinder()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var request = bindingContext.HttpContext.Request;

        // Start with default filter
        var filter = new PaginatedRequest();

        // ===== 1. Read from REQUEST BODY (JSON) =====
        if (request.ContentLength > 0 &&
            request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                using var reader = new StreamReader(request.Body);
                var bodyJson = await reader.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(bodyJson))
                {
                    var bodyFilter = JsonSerializer.Deserialize<PaginatedRequest>(bodyJson, _jsonOptions);
                    if (bodyFilter != null)
                    {
                        filter = bodyFilter;
                    }
                }
            }
            catch (JsonException ex)
            {
                // Log the error in production
                Console.WriteLine($"JSON parsing error: {ex.Message}");
            }
        }

        // ===== 2. Override with QUERY STRING values (if provided) =====

        // Page
        var pageValue = bindingContext.ValueProvider.GetValue("page").FirstOrDefault();
        if (int.TryParse(pageValue, out var pageInt) && pageInt > 0)
        {
            filter.Page = pageInt;
        }

        // PageSize
        var pageSizeValue = bindingContext.ValueProvider.GetValue("pageSize").FirstOrDefault();
        if (int.TryParse(pageSizeValue, out var pageSizeInt) && pageSizeInt > 0)
        {
            filter.PageSize = pageSizeInt;
        }

        // SearchTerm
        var searchTermValue = bindingContext.ValueProvider.GetValue("searchTerm").FirstOrDefault();
        if (!string.IsNullOrEmpty(searchTermValue))
        {
            filter.SearchTerm = searchTermValue;
        }

        // ===== 3. Handle Sorting from Query String =====
        // Only use query string sorting if body doesn't have any
        if (filter.SortExpressions == null || filter.SortExpressions.Count == 0)
        {
            filter.SortExpressions = ParseSortFromQuery(bindingContext.ValueProvider);
        }

        // ===== 4. Handle Simple Filters from Query String =====
        // These override body filters if provided
        ApplySimpleFiltersFromQuery(filter, bindingContext.ValueProvider);

        // ===== 5. Validate =====
        filter.Validate();

        bindingContext.Result = ModelBindingResult.Success(filter);
    }

    private static List<SortExpression> ParseSortFromQuery(IValueProvider valueProvider)
    {
        var sortExpressions = new List<SortExpression>();

        var sortValues = valueProvider.GetValue("sort").Values;
        var dirValues = valueProvider.GetValue("direction").Values;

        for (var i = 0; i < sortValues.Count; i++)
        {
            var prop = sortValues[i];
            if (string.IsNullOrEmpty(prop))
                continue;

            var dirStr = i < dirValues.Count ? dirValues[i] : "asc";

            sortExpressions.Add(new SortExpression
            {
                Property = prop,
                IsAscending = dirStr.Equals("asc", StringComparison.OrdinalIgnoreCase)
            });
        }

        return sortExpressions;
    }

    private static void ApplySimpleFiltersFromQuery(PaginatedRequest filter, IValueProvider valueProvider)
    {
        // ChecklistId
        var checklistIdValue = valueProvider.GetValue("checklistId").FirstOrDefault();
        if (Guid.TryParse(checklistIdValue, out var checklistId))
        {
            filter.ChecklistId = checklistId;
        }

        // ChecklistVersion
        var versionValue = valueProvider.GetValue("checklistVersion").FirstOrDefault();
        if (int.TryParse(versionValue, out var version))
        {
            filter.ChecklistVersion = version;
        }

        // FromDate
        var fromDateValue = valueProvider.GetValue("fromDate").FirstOrDefault();
        if (DateTime.TryParse(fromDateValue, out var fromDate))
        {
            filter.FromDate = fromDate;
        }

        // ToDate
        var toDateValue = valueProvider.GetValue("toDate").FirstOrDefault();
        if (DateTime.TryParse(toDateValue, out var toDate))
        {
            filter.ToDate = toDate;
        }
    }
}
