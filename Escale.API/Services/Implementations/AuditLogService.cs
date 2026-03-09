using Escale.API.Data;
using Escale.API.DTOs.AuditLogs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly EscaleDbContext _context;

    public AuditLogService(EscaleDbContext context)
    {
        _context = context;
    }

    public async Task<PagedAuditLogResponseDto> GetAuditLogsAsync(Guid organizationId, AuditLogQueryDto query)
    {
        var q = _context.AuditLogs.AsNoTracking()
            .Where(a => a.OrganizationId == organizationId);

        if (!string.IsNullOrEmpty(query.Action))
            q = q.Where(a => a.Action == query.Action);

        if (!string.IsNullOrEmpty(query.EntityType))
            q = q.Where(a => a.EntityType == query.EntityType);

        if (query.UserId.HasValue)
            q = q.Where(a => a.UserId == query.UserId);

        if (query.StartDate.HasValue)
            q = q.Where(a => a.Timestamp >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            q = q.Where(a => a.Timestamp <= query.EndDate.Value.AddDays(1));

        if (!string.IsNullOrEmpty(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(a => (a.UserName != null && a.UserName.ToLower().Contains(search))
                || (a.Details != null && a.Details.ToLower().Contains(search))
                || (a.EntityType != null && a.EntityType.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync();

        var items = await q.OrderByDescending(a => a.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                OrganizationId = a.OrganizationId,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return new PagedAuditLogResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
