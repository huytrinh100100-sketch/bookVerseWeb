using System.Security.Claims;
using BookVerse.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Id của người dùng hiện tại (từ claim NameIdentifier).</summary>
    protected Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    /// <summary>Id người dùng, hoặc Guid.Empty nếu request không xác thực.</summary>
    protected Guid GetCurrentUserId() => CurrentUserId;

    protected static IActionResult Ok<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                IsSuccess = true,
                Message = result.Message,
                Data = result.Data
            });
        }

        return new ObjectResult(new ApiResponse<T>
        {
            IsSuccess = false,
            Message = result.Message,
            Errors = result.Errors
        })
        {
            StatusCode = (int)result.StatusCode
        };
    }

    protected static IActionResult Created<T>(ServiceResult<T> result, string? location = null)
    {
        if (result.IsSuccess)
        {
            var response = new ApiResponse<T>
            {
                IsSuccess = true,
                Message = result.Message,
                Data = result.Data
            };

            return location is not null
                ? new CreatedResult(location, response)
                : new ObjectResult(response) { StatusCode = StatusCodes.Status201Created };
        }

        return new ObjectResult(new ApiResponse<T>
        {
            IsSuccess = false,
            Message = result.Message,
            Errors = result.Errors
        })
        {
            StatusCode = (int)result.StatusCode
        };
    }

    protected static IActionResult NoContent(ServiceResult<bool> result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return new ObjectResult(new ApiResponse<bool>
        {
            IsSuccess = false,
            Message = result.Message,
            Errors = result.Errors
        })
        {
            StatusCode = (int)result.StatusCode
        };
    }
}
