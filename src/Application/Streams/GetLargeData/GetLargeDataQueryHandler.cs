using System.IO;
using System.Runtime.CompilerServices;
using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Entities.Checklists;
using SharedKernel;

namespace Application.Streams.GetLargeData;


public class GetLargeDataQueryHandler(
    IBaseRepository<Checklist> _repository,
    IMapper _mapper
    ) : IQueryHandler<GetLargeDataQuery, IAsyncEnumerable<ChecklistDto>>
{ 
    //public async Task<Result<IAsyncEnumerable<ChecklistDto>>> Handle(
    //    GetLargeDataQuery  request,
    //    CancellationToken cancellationToken)
    //{
    //    var query = _repository.StreamAllAsync(request.Filter, cancellationToken);

    //    await foreach (var entity in query.WithCancellation(cancellationToken))
    //    {
    //        yield return _mapper.Map<ChecklistDto>(entity);
    //    }

    //    return Task.FromResult(Result.Success(query));
    //}




    // The Handle method now returns Task<Result<IAsyncEnumerable<TDto>>>
    public Task<Result<IAsyncEnumerable<ChecklistDto>>> Handle(
        GetLargeDataQuery request,
        CancellationToken cancellationToken)
    {
        var stream = StreamInternal(request, cancellationToken);
        return Task.FromResult(Result.Success(stream));
    }

    // This private method does the actual streaming and mapping
    private async IAsyncEnumerable<ChecklistDto> StreamInternal(
        GetLargeDataQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _repository.StreamAllAsync(request.Filter, cancellationToken);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            yield return _mapper.Map<ChecklistDto>(entity);
        }
    }

}


