using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/books")]
[Authorize]
public class BookController : BaseApiController
{
    private readonly IBookService _bookService;
    private readonly IBookCopyService _bookCopyService;

    public BookController(IBookService bookService, IBookCopyService bookCopyService)
    {
        _bookService = bookService;
        _bookCopyService = bookCopyService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.BookList)]
    public async Task<IActionResult> Search([FromQuery] BookSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [Authorize(Policy = PermissionConstants.BookView)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.BookCreate)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = PermissionConstants.BookUpdate)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateStatusAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Điều chỉnh tồn kho sách (FE: PUT /books/{id}/stock).</summary>
    [HttpPut("{id:guid}/stock")]
    [Authorize(Policy = PermissionConstants.BookAdjustStock)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateBookStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateStockAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Policy = PermissionConstants.BookDelete)]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.HardDeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = PermissionConstants.BookRestore)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.RestoreAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    // ----- Bản sao vật lý -----

    [HttpGet("{bookId:guid}/copies")]
    [Authorize(Policy = PermissionConstants.BookCopyList)]
    public async Task<IActionResult> GetCopies(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.GetByBookAsync(bookId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("copies")]
    [Authorize(Policy = PermissionConstants.BookCopyCreate)]
    public async Task<IActionResult> CreateCopies([FromBody] CreateBookCopyRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("copies/{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookCopyUpdate)]
    public async Task<IActionResult> UpdateCopy(Guid id, [FromBody] UpdateBookCopyRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("copies/{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookCopyDeleteTemp)]
    public async Task<IActionResult> DeleteCopy(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
