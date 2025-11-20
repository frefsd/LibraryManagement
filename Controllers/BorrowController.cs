using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 借阅管理模块
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        //依赖注入
        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryPage(int page = 1, int pageSize = 10)
        {
            var result = await _borrowService.GetPageAsync(page, pageSize);
            return Ok(new { code = true, data = new { rows = result.Rows, total = result.Total } });
        }

        /// <summary>
        /// 获取借阅信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequestDto dto)
        {
            if (!ModelState.IsValid)
                return Ok(new { code = false, msg = "参数无效" });

            await _borrowService.BorrowAsync(dto);
            return Ok(new { code = true, msg = "借阅成功" });
        }

        /// <summary>
        /// 获取图书归还信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Return(int id)
        {
            await _borrowService.ReturnAsync(id);
            return Ok(new { code = true, msg = "归还成功" });
        }
    }
}

