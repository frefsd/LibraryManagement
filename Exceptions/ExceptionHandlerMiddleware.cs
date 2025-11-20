using System.Text.Json;

namespace LibraryManagement.Exceptions
{
    /// <summary>
    /// 全局异常处理类
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public ExceptionHandlerMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<ExceptionHandlerMiddleware>>();
                var response = context.Response;
                response.ContentType = "application/json";

                int statusCode;
                string message;

                // 判断是否是业务异常（DomainException）
                if (ex is DomainException)
                {
                    // 业务异常：返回 400，暴露具体提示
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ex.Message;
                    logger.LogWarning(ex, "业务异常: {Message}", message);
                }
                else
                {
                    // 系统异常：返回 500，不暴露细节（安全）
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "服务器内部错误，请稍后重试。";
                    logger.LogError(ex, "未处理的系统异常");
                }

                response.StatusCode = statusCode;

                var result = new
                {
                    code = statusCode,
                    msg = message,
                    success = false
                };

                string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await response.WriteAsync(json);
            }
        }
    }
}
