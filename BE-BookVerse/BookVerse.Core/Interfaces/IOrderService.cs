using BookVerse.Core.Common;
using BookVerse.Core.Models.Order;

namespace BookVerse.Core.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<PagedResult<OrderListItemResponse>>> SearchAsync(OrderSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDetailResponse>> CreateAsync(CreateOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDetailResponse>> UpdateAsync(Guid id, UpdateOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDetailResponse>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> CancelAsync(Guid id, CancelOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RefundAsync(Guid id, RefundOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> MarkPaidAsync(Guid id, MarkPaidRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<OrderStatusHistoryResponse>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Xuất danh sách đơn hàng ra file CSV (FE: GET /orders/export).</summary>
    Task<ServiceResult<OrderExportResult>> ExportAsync(OrderSearchRequest request, CancellationToken cancellationToken = default);
}
