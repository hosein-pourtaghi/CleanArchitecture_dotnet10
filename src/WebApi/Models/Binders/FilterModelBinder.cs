using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Models.Binders;

/// <summary>
/// old
/// </summary>
//public class FilterModelBinder : IModelBinder
//{
//    public Task BindModelAsync(ModelBindingContext bindingContext)
//    {
//        var filter = new Filter();

//        // Simple properties
//        if (bindingContext.ValueProvider.GetValue("checklistId").FirstValue != null)
//            filter.ChecklistId = Guid.Parse(bindingContext.ValueProvider.GetValue("checklistId").FirstValue);

//        // Advanced conditions (via query string)
//        // ... (implementation omitted for brevity)

//        bindingContext.Result = ModelBindingResult.Success(filter);
//        return Task.CompletedTask;
//    }
//}
