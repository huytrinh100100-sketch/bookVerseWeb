using BookVerse.Core.Common;
using BookVerse.Core.Models.Dashboard;

namespace BookVerse.Core.Interfaces;

public interface IDashboardService
{
    Task<ServiceResult<DashboardOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<RevenueReportResponse>> GetRevenueReportAsync(RevenueReportRequest request, CancellationToken cancellationToken = default);
}
