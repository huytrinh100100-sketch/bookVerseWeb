using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/articles")]
[Authorize]
public class ArticleController : BaseApiController
{
    private readonly IContentService _contentService;

    public ArticleController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.ArticleList)]
    public async Task<IActionResult> Search([FromQuery] ArticleSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.SearchArticlesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ArticleView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentService.GetArticleAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [Authorize(Policy = PermissionConstants.ArticleView)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _contentService.GetArticleBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.ArticleCreate)]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.CreateArticleAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ArticleUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var result = await _contentService.UpdateArticleAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xuất bản bài viết (FE: PUT /articles/{id}/publish).</summary>
    [HttpPut("{id:guid}/publish")]
    [Authorize(Policy = PermissionConstants.ArticlePublish)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentService.PublishArticleAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ArticleDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentService.DeleteArticleAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
