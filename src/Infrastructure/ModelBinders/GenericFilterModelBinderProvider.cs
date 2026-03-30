using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApi.Models.Binders;

namespace Infrastructure.ModelBinders;

public class GenericFilterModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(PaginatedRequest) ||
            context.Metadata.ModelType.IsSubclassOf(typeof(PaginatedRequest)))
        {
            return new GenericFilterModelBinder();
        }

        return null;
    }
}
