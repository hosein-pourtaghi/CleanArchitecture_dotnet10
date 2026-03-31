using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Streams.StreamAll;
 
 

/// <summary>
/// Generic handler for streaming large datasets
/// </summary>
public class StreamAllHandler<TEntity, TDto> : IRequestHandler<StreamAllQuery<TEntity, TDto>, IAsyncEnumerable<TDto>>
    where TEntity : class
{
    private readonly IBaseRepository _repository;
    private readonly IMapper _mapper;

    public StreamAllHandler(IBaseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
     
    public async IAsyncEnumerable<TDto> Handle(
        StreamAllQuery<TEntity, TDto> request,
      [EnumeratorCancellation]
    CancellationToken cancellationToken)
    {
        var query = _repository.ApplyFiltering(
            _repository.GetQueryable<TEntity>(),
            request.Filter);

    

        // Stream results
        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var dto = _mapper.Map<TDto>(entity);
            yield return dto;
        }
    }
     
}
