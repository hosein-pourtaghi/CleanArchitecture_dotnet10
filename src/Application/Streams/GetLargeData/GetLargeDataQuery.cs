using Application.Common.DTOs.Checklists;
using Application.Common.DTOs.Shared;
using Application.Common.Messaging;

namespace Application.Streams.GetLargeData
{

    public sealed record GetLargeDataQuery(PaginatedRequest Filter) : IQuery<IAsyncEnumerable<ChecklistDto>>;
    //public class GetLargeDataQuery<TEntity, TDto> : IQuery<IAsyncEnumerable<TDto>>
    //    where TEntity : class
    //{
    //    public PaginatedRequest Filter { get; set; } = new();
    //    public string? AdditionalFilterJson { get; set; }
    //    public List<string>? Includes { get; set; }
    //}
}

