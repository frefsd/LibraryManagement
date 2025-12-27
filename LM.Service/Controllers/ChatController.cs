using LibraryManagement.LM.Pojo.DTO;
using LibraryManagement.LM.Service.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatMessageDto request, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        Response.Headers.Add("X-Accel-Buffering", "no");

        var writer = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);

        try
        {
            await foreach (var token in _chatService.AskStreamAsync(request.Message).WithCancellation(ct))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    // 确保返回正确的JSON字符串格式
                    var jsonString = JsonSerializer.Serialize(token);
                    await writer.WriteAsync($"data: {jsonString}\n\n");
                    await writer.FlushAsync(ct);
                }
            }

            // 发送结束标记
            await writer.WriteAsync("data: [DONE]\n\n");
            await writer.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // 客户端取消连接是正常的
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize($"错误：{ex.Message}");
            await writer.WriteAsync($"data: {errorJson}\n\n");
            await writer.FlushAsync(ct);
        }
    }
}