using LibraryManagement.DTO;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// AI智能聊天
    /// </summary>
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        //依赖注入
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// 采用流式打印输出
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("stream")]
        public async Task streamMessage([FromBody] ChatMessageDto dto)
        {
            //禁用响应缓冲，立即输出
            Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "Keep-alive");

            var fullReply = await _chatService.AskAsync(dto.Message);

            //模拟“打字”效果，逐字发送
            foreach (var item in fullReply)
            {
                await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(item.ToString()));
                await Response.Body.FlushAsync();
                await Task.Delay(20); //控制打字速度
            }
           
        }
    }
}
