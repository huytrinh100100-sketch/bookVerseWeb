using System.Net;
using System.Text.Json;
using BookVerse.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.WebAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang.", (List<string>?)null),
            DbUpdateException => (HttpStatusCode.BadRequest, "Không thể lưu dữ liệu. Có thể dữ liệu đã tồn tại hoặc vi phạm ràng buộc.", (List<string>?)null),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Bạn không có quyền thực hiện thao tác này.", (List<string>?)null),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message, (List<string>?)null),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Không tìm thấy dữ liệu yêu cầu.", (List<string>?)null),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "Yêu cầu bị hủy do quá thời gian chờ.", (List<string>?)null),
            _ => (HttpStatusCode.InternalServerError, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.", (List<string>?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Lỗi không mong đợi tại {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex, "Lỗi nghiệp vụ tại {Path}: {Message}", context.Request.Path, ex.Message);
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiResponse<object>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        });
    }
}
