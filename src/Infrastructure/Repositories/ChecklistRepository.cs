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

    public async Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true)
    {
        var query = _context.Checklists
            .Where(c => c.Id == id)
            .AsNoTracking();

        if (includeGroups)
        {
            query = query.Include(c => c.Groups)
                         .ThenInclude(g => g.Children)
                         .ThenInclude(g => g.Questions)
                         .ThenInclude(q => q.Options);
        }

        return await query.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Checklist not found");
    }
    public async Task<Checklist> GetByVersionAsync(Guid checklistId, int version)
    {
        return await _context.Checklists
            .Include(c => c.Groups)
                .ThenInclude(g => g.Children)
                .ThenInclude(g => g.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c =>
                c.Id == checklistId &&
                c.Version == version
            ) ?? throw new KeyNotFoundException(
                $"Checklist version {version} not found for checklist {checklistId}"
            );
    }
    public async Task<Checklist> AddAsync(Checklist checklist)
    {
        _context.Checklists.Add(checklist);
        await _context.SaveChangesAsync();
        return checklist;

    }
    public async Task<Checklist> UpdateAsync(Guid id, Checklist newStructure)
    {
        var checklist = await GetByIdAsync(id, true);

        // Create new version BEFORE updating
        var newVersion = checklist.CreateNewVersion();

        // Update current checklist with new structure
        checklist.UpdateStructure(newStructure);

        // Save both versions
        _context.Checklists.Update(checklist);
        _context.Checklists.Add(newVersion);

        await _context.SaveChangesAsync();
        return checklist;
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
