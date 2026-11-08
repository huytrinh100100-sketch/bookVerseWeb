using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Dashboard;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<DashboardOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var validOrders = _context.Orders.Where(x => !x.IsDeleted
            && x.OrderStatus != OrderStatus.Cancelled
            && x.OrderStatus != OrderStatus.Refunded);

        var totalRevenue = await validOrders.SumAsync(x => (decimal?)x.FinalAmount, cancellationToken) ?? 0m;
        var revenueThisMonth = await validOrders.Where(x => x.OrderDate >= monthStart).SumAsync(x => (decimal?)x.FinalAmount, cancellationToken) ?? 0m;
        var revenueLastMonth = await validOrders.Where(x => x.OrderDate >= lastMonthStart && x.OrderDate < monthStart).SumAsync(x => (decimal?)x.FinalAmount, cancellationToken) ?? 0m;

        var totalOrders = await validOrders.CountAsync(cancellationToken);
        var ordersThisMonth = await validOrders.CountAsync(x => x.OrderDate >= monthStart, cancellationToken);
        var ordersLastMonth = await validOrders.CountAsync(x => x.OrderDate >= lastMonthStart && x.OrderDate < monthStart, cancellationToken);

        var totalBooks = await _context.Books.CountAsync(x => !x.IsDeleted, cancellationToken);
        var publishedBooks = await _context.Books.CountAsync(x => !x.IsDeleted && x.Status == BookStatus.Published, cancellationToken);
        var totalCustomers = await _context.Users.CountAsync(x => !x.IsDeleted, cancellationToken);

        var inventoryValue = await _context.Books
            .Where(x => !x.IsDeleted)
            .SumAsync(x => (decimal?)(x.TotalQuantity * x.PriceForSell), cancellationToken) ?? 0m;

        var lowStockThreshold = 5;
        var lowStockCount = await _context.Books.CountAsync(x => !x.IsDeleted && x.StockForSell <= lowStockThreshold, cancellationToken);

        var revenueTrend = revenueLastMonth > 0
            ? Math.Round((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100m, 1)
            : 0m;
        var orderTrend = ordersLastMonth > 0
            ? Math.Round((ordersThisMonth - ordersLastMonth) * 100m / ordersLastMonth, 1)
            : 0m;

        var metrics = new List<DashboardMetricResponse>
        {
            new()
            {
                Label = "Doanh thu thang",
                Value = FormatCurrency(revenueThisMonth),
                TrendValue = BuildTrend(revenueTrend, "so voi thang truoc"),
                TrendDirection = ResolveDirection(revenueTrend),
                Icon = "pi pi-wallet",
                Tone = "success"
            },
            new()
            {
                Label = "Don hang moi",
                Value = ordersThisMonth.ToString("N0"),
                TrendValue = BuildTrend(orderTrend, "so voi thang truoc"),
                TrendDirection = ResolveDirection(orderTrend),
                Icon = "pi pi-receipt",
                Tone = "primary"
            },
            new()
            {
                Label = "Gia tri ton kho",
                Value = FormatCurrency(inventoryValue),
                TrendValue = $"{totalBooks} tieu de dang kinh doanh",
                TrendDirection = "flat",
                Icon = "pi pi-box",
                Tone = "info"
            },
            new()
            {
                Label = "Can nhac muc thap",
                Value = $"{lowStockCount} tieu de",
                TrendValue = lowStockCount > 0 ? "Can nhap gap hom nay" : "Ton kho dang on dinh",
                TrendDirection = lowStockCount > 0 ? "down" : "flat",
                Icon = "pi pi-exclamation-triangle",
                Tone = lowStockCount > 0 ? "danger" : "success"
            }
        };

        // ----- Hieu luc theo danh muc -----
        var categoryRows = await (
            from detail in _context.OrderDetails.AsNoTracking()
            join order in _context.Orders.AsNoTracking() on detail.OrderId equals order.Id
            join book in _context.Books.AsNoTracking() on detail.BookId equals book.Id
            join category in _context.CommonCategories.AsNoTracking() on book.CategoryId equals category.Id
            where !order.IsDeleted && order.OrderStatus != OrderStatus.Cancelled && order.OrderStatus != OrderStatus.Refunded
            group detail by new { category.Name } into g
            select new
            {
                CategoryName = g.Key.Name,
                Revenue = g.Sum(x => x.LineTotal),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync(cancellationToken);

        var totalCategoryRevenue = categoryRows.Sum(x => x.Revenue);

        var categoryPerformance = categoryRows
            .Select(x => new CategoryPerformanceResponse
            {
                CategoryName = x.CategoryName,
                Revenue = ServiceHelpers.RoundMoney(x.Revenue),
                OrderCount = x.OrderCount,
                SharePercent = totalCategoryRevenue > 0
                    ? Math.Round(x.Revenue / totalCategoryRevenue * 100m, 1)
                    : 0m
            })
            .ToList();

        // ----- Don hang moi nhat -----
        var recentOrders = await _context.Orders
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.OrderDate)
            .Take(10)
            .Select(x => new RecentOrderResponse
            {
                Id = x.Id,
                Code = x.Code,
                CustomerName = x.Customer.FullName,
                PreviewItem = x.Details.OrderBy(d => d.CreatedAt).Select(d => d.BookTitle).FirstOrDefault() ?? string.Empty,
                Amount = x.FinalAmount,
                PaymentMethod = x.PaymentMethod.ToString(),
                OrderStatus = x.OrderStatus.ToString(),
                OrderDate = x.OrderDate
            })
            .ToListAsync(cancellationToken);

        // ----- Can nhac muc thap -----
        var lowStockAlerts = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.StockForSell <= lowStockThreshold)
            .OrderBy(x => x.StockForSell)
            .Take(10)
            .Select(x => new LowStockBookResponse
            {
                BookId = x.Id,
                Title = x.Title,
                StockForSell = x.StockForSell,
                StockForRent = x.StockForRent,
                WarehouseName = x.Copies.Where(c => !c.IsDeleted).Select(c => c.Warehouse!.Name).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var revenueSeries = await BuildRevenueChartAsync(monthStart, cancellationToken);

        return ServiceResult<DashboardOverviewResponse>.Success(new DashboardOverviewResponse
        {
            Metrics = metrics,
            CategoryPerformance = categoryPerformance,
            RecentOrders = recentOrders,
            LowStockAlerts = lowStockAlerts,
            RevenueSeries = revenueSeries
        }, "Lấy dữ liệu bảng tổng quan thành công.");
    }

    public async Task<ServiceResult<RevenueReportResponse>> GetRevenueReportAsync(RevenueReportRequest request, CancellationToken cancellationToken = default)
    {
        var from = request.FromDate == default ? DateTime.UtcNow.AddDays(-30) : request.FromDate;
        var to = request.ToDate == default ? DateTime.UtcNow : request.ToDate;

        var validOrders = _context.Orders
            .Where(x => !x.IsDeleted && x.OrderStatus != OrderStatus.Cancelled && x.OrderStatus != OrderStatus.Refunded)
            .Where(x => x.OrderDate >= from && x.OrderDate <= to);

        var totalRevenue = await validOrders.SumAsync(x => (decimal?)x.FinalAmount, cancellationToken) ?? 0m;
        var totalShipping = await validOrders.SumAsync(x => (decimal?)x.ShippingAmount, cancellationToken) ?? 0m;
        var totalDiscount = await validOrders.SumAsync(x => (decimal?)x.DiscountAmount, cancellationToken) ?? 0m;
        var totalOrders = await validOrders.CountAsync(cancellationToken);

        var grouped = request.GroupBy?.ToLowerInvariant() switch
        {
            "month" => await validOrders
                .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                .Select(g => new ChartPointResponse
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Value = g.Sum(x => x.FinalAmount)
                })
                .ToListAsync(cancellationToken),
            "year" => await validOrders
                .GroupBy(x => x.OrderDate.Year)
                .Select(g => new ChartPointResponse
                {
                    Label = g.Key.ToString(),
                    Value = g.Sum(x => x.FinalAmount)
                })
                .ToListAsync(cancellationToken),
            _ => await validOrders
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new ChartPointResponse
                {
                    Label = g.Key.ToString("dd/MM/yyyy"),
                    Value = g.Sum(x => x.FinalAmount)
                })
                .ToListAsync(cancellationToken)
        };

        var items = grouped.OrderBy(x => x.Label).ToList();

        return ServiceResult<RevenueReportResponse>.Success(new RevenueReportResponse
        {
            TotalRevenue = ServiceHelpers.RoundMoney(totalRevenue),
            TotalDiscount = ServiceHelpers.RoundMoney(totalDiscount),
            TotalShipping = ServiceHelpers.RoundMoney(totalShipping),
            TotalOrders = totalOrders,
            Items = items
        }, "Lấy báo cáo doanh thu thành công.");
    }

    /// <summary>Định dạng tiền kiểu "842.600.000 d".</summary>
    private static string FormatCurrency(decimal value)
    {
        return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".") + " d";
    }

    /// <summary>Dựng chuỗi trend so sánh (ví dụ "+8.2% so voi thang truoc").</summary>
    private static string BuildTrend(decimal percent, string suffix)
    {
        var sign = percent > 0 ? "+" : string.Empty;
        return $"{sign}{percent.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}% {suffix}";
    }

    /// <summary>Chuyển phần trăm sang chiều trend (up / down / flat).</summary>
    private static string ResolveDirection(decimal percent)
    {
        if (percent > 0) return "up";
        if (percent < 0) return "down";
        return "flat";
    }

    private async Task<IReadOnlyCollection<ChartPointResponse>> BuildRevenueChartAsync(DateTime monthStart, CancellationToken cancellationToken = default)
    {
        var points = new List<ChartPointResponse>();

        for (var i = 0; i < 6; i++)
        {
            var start = monthStart.AddMonths(i - 5);
            var end = start.AddMonths(1);

            var revenue = await _context.Orders
                .Where(x => !x.IsDeleted && x.OrderStatus != OrderStatus.Cancelled && x.OrderStatus != OrderStatus.Refunded)
                .Where(x => x.OrderDate >= start && x.OrderDate < end)
                .SumAsync(x => (decimal?)x.FinalAmount, cancellationToken) ?? 0m;

            points.Add(new ChartPointResponse
            {
                Label = $"T{start.Month}",
                Value = ServiceHelpers.RoundMoney(revenue)
            });
        }

        return points;
    }

    private async Task<IReadOnlyCollection<ChartPointResponse>> BuildOrdersChartAsync(DateTime monthStart, CancellationToken cancellationToken = default)
    {
        var points = new List<ChartPointResponse>();

        for (var i = 0; i < 6; i++)
        {
            var start = monthStart.AddMonths(i - 5);
            var end = start.AddMonths(1);

            var count = await _context.Orders
                .Where(x => !x.IsDeleted && x.OrderStatus != OrderStatus.Cancelled)
                .CountAsync(x => x.OrderDate >= start && x.OrderDate < end, cancellationToken);

            points.Add(new ChartPointResponse
            {
                Label = $"T{start.Month}",
                Value = count
            });
        }

        return points;
    }
}
