using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 图书管理模块
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        //依赖注入
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryPage(string? begin, string? end, string? name, int page = 1, int pageSize = 10)
        {
            var result = await _bookService.GetPageAsync(begin, end, name, page, pageSize);
            return Ok(new {code = true, data = result});
        }

        /// <summary>
        /// 添加图书
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody]Book book)
        {
            await _bookService.AddAsync(book);
            return Ok(new { code = true, msg = "图书添加成功" });
        }

        /// <summary>
        /// 根据id获取图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryInfo(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null)
                return NotFound(new { code = false, msg = "图书不存在" });
            return Ok(new { code = true, data = book });
        }

        /// <summary>
        /// 修改图书信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody]Book book)
        {
            await _bookService.UpdateAsync(book);
            return Ok(new { code = true, msg = "修改成功" });
        }

        /// <summary>
        /// 删除图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _bookService.DeleteAsync(id);
            return Ok(new { code = true, msg = "删除成功" });
        }
    }
}