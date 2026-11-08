using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Bảng tổng quan điều hành (doanh thu, đơn hàng, sách bán chạy, tồn kho thấp).</summary>
    [HttpGet("overview")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Báo cáo doanh thu theo khoảng thời gian (groupBy: day | month | year).</summary>
    [HttpGet("revenue-report")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportRequest request, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetRevenueReportAsync(request, cancellationToken);
        return Ok(result);
    }
}
