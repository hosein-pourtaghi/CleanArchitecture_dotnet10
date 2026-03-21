using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Models.Binders;
// Infrastructure/ModelBinders/GenericFilterModelBinder.cs
public class GenericFilterModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var filter = (Filter)Activator.CreateInstance(bindingContext.ModelType);

        // Handle common properties
        if (bindingContext.ValueProvider.GetValue("page").FirstValue != null)
            filter.Page = int.Parse(bindingContext.ValueProvider.GetValue("page").FirstValue);

        if (bindingContext.ValueProvider.GetValue("pageSize").FirstValue != null)
            filter.PageSize = int.Parse(bindingContext.ValueProvider.GetValue("pageSize").FirstValue);

        if (bindingContext.ValueProvider.GetValue("searchTerm").FirstValue != null)
            filter.SearchTerm = bindingContext.ValueProvider.GetValue("searchTerm").FirstValue;

        // Handle sorting
        var sortProperties = bindingContext.ValueProvider.GetValue("sort");
        var directionProperties = bindingContext.ValueProvider.GetValue("direction");
        if (sortProperties != ValueProviderResult.None && directionProperties != ValueProviderResult.None)
        {
            for (int i = 0; i < sortProperties.Count(); i++)
            {
                var sortProperty =sortProperties[i];
                var direction = directionProperties[i];
                bool isAscending = direction.Equals("asc", StringComparison.OrdinalIgnoreCase);
                filter.SortExpressions.Add(new SortExpression { Property = sortProperty, IsAscending = isAscending });
            }
        }

        // Handle advanced conditions
        var filterConditions = bindingContext.ValueProvider.GetValue("filter");
        if (filterConditions != ValueProviderResult.None)
        {
            foreach (var condition in filterConditions)
            {
                var parts = condition.Split(',');
                if (parts.Length >= 4)
                {
                    filter.FilterConditions.Add(new FilterCondition
                    {
                        Property = parts[0],
                        Value = parts[1],
                        Operator = Enum.Parse<FilterOperator>(parts[2], true),
                        Logic = Enum.Parse<FilterLogic>(parts[3], true)
                    });
                }
            }
        }

        filter.Validate();
        bindingContext.Result = ModelBindingResult.Success(filter);
        return Task.CompletedTask;
    }
}


public class GenericFilterModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType.IsSubclassOf(typeof(Filter)))
        {
            return new GenericFilterModelBinder();
        }
        return null;
    }
}
