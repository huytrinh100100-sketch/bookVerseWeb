using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/orders")]
[Authorize]
public class OrderController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.OrderList)]
    public async Task<IActionResult> Search([FromQuery] OrderSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xuất danh sách đơn hàng ra CSV (FE: GET /orders/export).</summary>
    [HttpGet("export")]
    [Authorize(Policy = PermissionConstants.OrderExport)]
    public async Task<IActionResult> Export([FromQuery] OrderSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.ExportAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            return Ok(result);
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(result.Data.Content))
            .ToArray();

        return File(bytes, result.Data.ContentType, result.Data.FileName);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.OrderView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.OrderCreate)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.OrderProcess)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = PermissionConstants.OrderProcess)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateStatusAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Policy = PermissionConstants.OrderCancel)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/refund")]
    [Authorize(Policy = PermissionConstants.OrderRefund)]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.RefundAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-paid")]
    [Authorize(Policy = PermissionConstants.OrderProcess)]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.MarkPaidAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = PermissionConstants.OrderView)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetHistoryAsync(id, cancellationToken);
        return Ok(result);
    }
}
