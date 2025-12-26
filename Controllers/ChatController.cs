using LibraryManagement.DTO;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// AI智能聊天
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        //依赖注入
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> sendMessage([FromBody] ChatMessageDto dto)
        {
            var reply = await _chatService.AskAsync(dto.Message);
            return Ok(new { reply });
        }
    }
}
