using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 出版社类
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class PublisherController: ControllerBase
    {
        private readonly IPublisherService _publisherService;
        
        //依赖注入
        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var publisher = await _publisherService.AllAsync();
            return Ok(new { code = true, data = publisher });
        }
    }
}
