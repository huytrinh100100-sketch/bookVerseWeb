using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/reviews")]
[Authorize]
public class ReviewController : BaseApiController
{
    private readonly IContentService _contentService;

    public ReviewController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.ReviewList)]
    public async Task<IActionResult> Search([FromQuery] ReviewSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.SearchReviewsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ReviewView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentService.GetReviewAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Khách hàng viết đánh giá sách.</summary>
    [HttpPost]
    [Authorize(Policy = PermissionConstants.ShopReview)]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.CreateReviewAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    /// <summary>Duyệt / từ chối đánh giá và trả lời.</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = PermissionConstants.ReviewApprove)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateReviewStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.UpdateReviewStatusAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Duyệt đánh giá (FE: PUT /reviews/{id}/approve).</summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = PermissionConstants.ReviewApprove)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.ApproveReviewAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Từ chối đánh giá (FE: PUT /reviews/{id}/reject).</summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = PermissionConstants.ReviewReject)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.RejectReviewAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Đánh dấu đánh giá vi phạm (FE: PUT /reviews/{id}/flag).</summary>
    [HttpPut("{id:guid}/flag")]
    [Authorize(Policy = PermissionConstants.ReviewApprove)]
    public async Task<IActionResult> Flag(Guid id, [FromBody] FlagReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.FlagReviewAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ReviewDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentService.DeleteReviewAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
