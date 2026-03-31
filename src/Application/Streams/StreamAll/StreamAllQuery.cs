using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.DTOs.Shared;
using MediatR;

namespace Application.Streams.StreamAll;
  

/// <summary>
/// Generic streaming request for paginated/filtered data
/// </summary>
public class StreamAllQuery<TEntity, TDto> :IRequest<IAsyncEnumerable<TDto>>
      where TEntity : class
{
    public PaginatedRequest Filter { get; set; } = new();
     
     
}
