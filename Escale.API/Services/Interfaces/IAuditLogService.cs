using Escale.API.DTOs.AuditLogs;

namespace Escale.API.Services.Interfaces;

public interface IAuditLogService
{
    Task<PagedAuditLogResponseDto> GetAuditLogsAsync(Guid organizationId, AuditLogQueryDto query);
}
