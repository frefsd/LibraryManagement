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
                //记录日志
                var logger = context.RequestServices.GetRequiredService<ILogger<ExceptionHandlerMiddleware>>();
                logger.LogError(ex, "发生未处理的异常");

                //统一返回JSON错误
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = StatusCodes.Status400BadRequest;

                var result = new
                {
                    code = 400,
                    msg = ex.Message,
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
