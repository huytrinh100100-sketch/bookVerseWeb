using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

/// <summary>
/// Bản sao vật lý - đúng route FE-Bookstore gọi: /api/v1/book-copies
/// </summary>
[Route("api/v1/book-copies")]
[Authorize]
public class BookCopyController : BaseApiController
{
    private readonly IBookCopyService _bookCopyService;

    public BookCopyController(IBookCopyService bookCopyService)
    {
        _bookCopyService = bookCopyService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.BookCopyList)]
    public async Task<IActionResult> Search([FromQuery] BookCopySearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.SearchCopiesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("books/{bookId:guid}")]
    [Authorize(Policy = PermissionConstants.BookCopyList)]
    public async Task<IActionResult> GetByBook(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.GetByBookAsync(bookId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.BookCopyCreate)]
    public async Task<IActionResult> Create([FromBody] CreateBookCopyRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookCopyUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookCopyRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.BookCopyDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookCopyService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
