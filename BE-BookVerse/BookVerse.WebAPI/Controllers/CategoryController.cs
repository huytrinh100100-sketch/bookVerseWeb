using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/categories")]
[Authorize]
public class CategoryController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.CategoryList)]
    public async Task<IActionResult> Search([FromQuery] CategorySearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cây danh mục dạng lồng nhau (Children).</summary>
    [HttpGet("tree")]
    [Authorize(Policy = PermissionConstants.CategoryList)]
    public async Task<IActionResult> GetTree([FromQuery] string? rootCode, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetTreeAsync(rootCode, cancellationToken);
        return Ok(result);
    }

    /// <summary>Danh sách nhóm danh mục gốc (book-category, article-category...).</summary>
    [HttpGet("roots")]
    [Authorize(Policy = PermissionConstants.CategoryList)]
    public async Task<IActionResult> GetRoots(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetRootsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Danh sách danh mục theo mã nhóm (ví dụ: book-category).</summary>
    [HttpGet("by-root/{rootCode}")]
    [Authorize(Policy = PermissionConstants.CategoryList)]
    public async Task<IActionResult> GetByRoot(string rootCode, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetItemsByRootCodeAsync(rootCode, activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CategoryView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.CategoryCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CategoryUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/move")]
    [Authorize(Policy = PermissionConstants.CategoryUpdate)]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.MoveAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = PermissionConstants.CategoryUpdate)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCategoryStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateStatusAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reorder")]
    [Authorize(Policy = PermissionConstants.CategoryReorder)]
    public async Task<IActionResult> Reorder([FromBody] ReorderCategoriesRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.ReorderAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CategoryDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Policy = PermissionConstants.CategoryDelete)]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.HardDeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = PermissionConstants.CategoryRestore)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.RestoreAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
