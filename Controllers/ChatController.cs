using LibraryManagement.DTO;
using LibraryManagement.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly QwenApiService _qwenService;

    public ChatController(QwenApiService qwenService)
    {
        _qwenService = qwenService;
    }

    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatMessageDto request, CancellationToken ct)
    {
        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.Add("Cache-Control", "no-cache");

        var writer = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);

        await foreach (var token in _qwenService.GenerateResponseStreamAsync(request.Message).WithCancellation(ct))
        {
            await writer.WriteAsync(token);
            await writer.FlushAsync(ct); // 确保立即发送
        }

        await writer.FlushAsync(ct);
    }
}