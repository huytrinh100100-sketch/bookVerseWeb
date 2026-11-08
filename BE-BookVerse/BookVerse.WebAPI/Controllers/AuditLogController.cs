using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.AuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/audit-logs")]
[Authorize]
public class AuditLogController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.AuditLogList)]
    public async Task<IActionResult> Search([FromQuery] AuditLogSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuditLogList)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }
}
