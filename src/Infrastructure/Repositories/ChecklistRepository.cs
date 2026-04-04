using Application.Abstractions.Interfaces.Checklists;
using AutoMapper;
using Domain.Checklists;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChecklistRepository : BaseRepository<Checklist>, IChecklistRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ChecklistRepository(ApplicationDbContext context, IMapper mapper) :
        base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true, bool asNoTracking = true)
    {
        var query = _context.Checklists
            .Where(c => c.Id == id)
            ;
        if (asNoTracking == true)
        {
            query = query.AsNoTracking();
        }

        if (includeGroups)
        {
            query = query
                .Include(c => c.Groups).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
                .Include(c => c.Groups).ThenInclude(g => g.Children).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
                ;
        }

        return await query.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Checklist not found");
    }
    public async Task<Checklist> GetByVersionAsync(Guid checklistId, int version)
    {
        return await _context.Checklists
            .Include(c => c.Groups).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
            .Include(c => c.Groups).ThenInclude(g => g.Children).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c =>
                c.Id == checklistId &&
                c.Version == version
            ) ?? throw new KeyNotFoundException(
                $"Checklist version {version} not found for checklist {checklistId}"
            );
    }

    public async Task<Checklist> UpdateAsync(Guid id, Checklist newStructure)
    {
        try
        {
            var checklist = await GetByIdAsync(id, true, false);
             
            // Update current checklist with new structure
            checklist.UpdateStructure(newStructure);
             
            //_context.Checklists.Update(checklist);

            await _context.SaveChangesAsync();

            return checklist;
        }
        catch (Exception e)
        {

            throw;
        }
    }
    public async Task DeleteAsync(Guid id)
    {
        var checklist = await _context.Checklists.FindAsync(id);
        if (checklist != null)
        {
            _context.Checklists.Remove(checklist);
            await _context.SaveChangesAsync();
        }
    }
}
