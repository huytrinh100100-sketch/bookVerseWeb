namespace BookVerse.Core.Models.Dashboard;

/// <summary>Shape dashboard đúng theo FE (DashboardOverviewResponse).</summary>
public class DashboardOverviewResponse
{
    public IReadOnlyCollection<DashboardMetricResponse> Metrics { get; set; } = [];
    public IReadOnlyCollection<CategoryPerformanceResponse> CategoryPerformance { get; set; } = [];
    public IReadOnlyCollection<RecentOrderResponse> RecentOrders { get; set; } = [];
    public IReadOnlyCollection<LowStockBookResponse> LowStockAlerts { get; set; } = [];
    public IReadOnlyCollection<ChartPointResponse> RevenueSeries { get; set; } = [];
}

public class DashboardMetricResponse
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string TrendValue { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = "flat";
    public string Icon { get; set; } = string.Empty;
    public string Tone { get; set; } = "primary";
}

public class CategoryPerformanceResponse
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal SharePercent { get; set; }
}

public class ChartPointResponse
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? SecondaryValue { get; set; }
}

public class TopItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SubName { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PreviewItem { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

public class LowStockBookResponse
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public string? WarehouseName { get; set; }
}

public class RevenueReportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string GroupBy { get; set; } = "day";
}

public class RevenueReportResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalShipping { get; set; }
    public decimal GrossProfit { get; set; }
    public int TotalOrders { get; set; }
    public IReadOnlyCollection<ChartPointResponse> Items { get; set; } = [];
}
