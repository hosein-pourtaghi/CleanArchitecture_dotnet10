using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Data;
using Application.Common.Interfaces.Checklists;
using AutoMapper;
using Domain.Checklists;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
 

//public class ChecklistRepository2 : AdvancedRepository<ApplicationDbContext, Checklist>, IChecklistRepository2
//{
//    private readonly ApplicationDbContext _context;
//    private readonly IMapper _mapper;

//    public ChecklistRepository2(ApplicationDbContext context, IMapper mapper) : base(context)
//    {
//        _context = context;
//        _mapper = mapper;
//    }

//    public async Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true, bool asNoTracking = false)
//    {
//        var query = _context.Checklists.Where(c => c.Id == id);

//        if (asNoTracking)
//            query = query.AsNoTracking();

//        if (includeGroups)
//        {
//            query = query
//                .Include(c => c.Groups)
//                    .ThenInclude(g => g.Questions)
//                    .ThenInclude(q => q.Options)
//                .Include(c => c.Groups)
//                    .ThenInclude(g => g.Children)
//                        .ThenInclude(g => g.Questions)
//                        .ThenInclude(q => q.Options);
//        }

//        return await query.FirstOrDefaultAsync()
//            ?? throw new KeyNotFoundException("Checklist not found");
//    }

//    public async Task<Checklist> GetByVersionAsync(Guid checklistId, int version)
//    {
//        return await _context.Checklists
//            .Include(c => c.Groups)
//                .ThenInclude(g => g.Questions)
//                .ThenInclude(q => q.Options)
//            .Include(c => c.Groups)
//                .ThenInclude(g => g.Children)
//                    .ThenInclude(g => g.Questions)
//                    .ThenInclude(q => q.Options)
//            .FirstOrDefaultAsync(c => c.Id == checklistId && c.Version == version)
//            ?? throw new KeyNotFoundException($"Checklist version {version} not found");
//    }

//    /// <summary>
//    /// Simple update without versioning - uses generic base method
//    /// </summary>
//    public async Task<Checklist> UpdateSimpleAsync(Guid id, Checklist newStructure)
//    {
//        return await UpdateAggregateAsync(id, newStructure);
//    }

//    /// <summary>
//    /// Update with custom configuration
//    /// </summary>
//    public async Task<Checklist> UpdateWithConfigAsync(Guid id, Checklist newStructure)
//    {
//        var configuration = new AggregateConfiguration<Checklist>
//        {
//            ExcludedProperties = new List<string>
//            {
//                "Id",
//                "Version",
//                "CreatedDate",
//                "CreatedBy"
//            },
//            PreSaveAction = (existing, newEntity, context) =>
//            {
//                // Update LastModified
//                var prop = typeof(Checklist).GetProperty("LastModified");
//                prop?.SetValue(existing, DateTime.UtcNow);
//            },
//            MaxNestingDepth = 3
//        };

//        return await UpdateAggregateAsync(id, newStructure, configuration);
//    }

//    protected override AggregateConfiguration<Checklist> CreateDefaultConfiguration()
//    {
//        return new AggregateConfiguration<Checklist>
//        {
//            ExcludedProperties = new List<string>
//            {
//                "Id",
//                "Version",
//                "CreatedDate",
//                "CreatedBy"
//            },
//            PreSaveAction = (existing, newEntity, context) =>
//            {
//                var prop = typeof(Checklist).GetProperty("LastModified");
//                prop?.SetValue(existing, DateTime.UtcNow);
//            }
//        };
//    }
//}
