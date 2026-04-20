using Application.Common.Interfaces.Core;
using Domain.Aggregates.Checklists;

namespace Application.Common.Interfaces.Checklists;

public interface IChecklistRepository : IBaseRepository<Checklist>
{
    Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true, bool asNoTracking = true);
    Task<Checklist> GetByVersionAsync(Guid checklistId, int version);
    Task<Checklist> UpdateAsync(Guid id, Checklist newStructure);

    Task DeleteAsync(Guid id);



}
