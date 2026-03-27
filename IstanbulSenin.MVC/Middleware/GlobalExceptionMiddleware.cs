using IstanbulSenin.BLL.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IstanbulSenin.MVC.Middleware
{
    /// <summary>
    /// Küresel hata yönetim middleware'i
    /// Tüm API hataları standart format'ta dönülür
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Küresel hata yakalaması: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ErrorResponseDto
            {
                Code = "INTERNAL_ERROR",
                Message = "Sunucu taraflı bir hata oluştu"
            };

            if (exception is ArgumentException argEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "VALIDATION_ERROR";
                response.Message = argEx.Message;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
