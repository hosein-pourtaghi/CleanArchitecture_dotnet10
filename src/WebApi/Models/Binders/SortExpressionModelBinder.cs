using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Models.Binders;

/// <summary>
/// old
/// </summary>
//public class SortExpressionModelBinder : IModelBinder
//{
//    public Task BindModelAsync(ModelBindingContext bindingContext)
//    {
//        var sortValues = bindingContext.ValueProvider.GetValue("sort");
//        var directionValues = bindingContext.ValueProvider.GetValue("direction");

//        if (sortValues == ValueProviderResult.None || directionValues == ValueProviderResult.None)
//            return Task.CompletedTask;

//        var sortExpressions = new List<SortExpression>();
//        for (int i = 0; i < sortValues.Count; i++)
//        {
//            sortExpressions.Add(new SortExpression
//            {
//                Property = sortValues[i],
//                IsAscending = directionValues[i].Equals("asc", StringComparison.OrdinalIgnoreCase)
//            });
//        }

//        bindingContext.Result = ModelBindingResult.Success(sortExpressions);
//        return Task.CompletedTask;
//    }
//}

