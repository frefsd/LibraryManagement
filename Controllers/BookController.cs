using LibraryManagement.DTO;
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
        private readonly IBorrowService _borrowService;
        private readonly IOssService _ossService;

        //依赖注入
        public BookController(IBookService bookService, IBorrowService borrowService, IOssService ossService)
        {
            _bookService = bookService;
            _borrowService = borrowService;
            _ossService = ossService;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryPage(string? begin, string? end, string? name, int page = 1, int pageSize = 10)
        {
            var result = await _bookService.GetPageAsync(begin, end, name, page, pageSize);
            return Ok(new { code = true, data = result });
        }

        /// <summary>
        /// 添加图书
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] CreateBookRequest request)
        {
            //验证模型
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { code = false, msg = "书名不能为空"});

            string? coverUrl = null;
            if (request.CoverFile != null)
            {
                try
                {
                    //图片上传到aliyun的文件夹名称
                    coverUrl = await _ossService.UploadFileAsync(request.CoverFile, "bok-covers");
                }
                catch ( Exception ex)
                {
                    return BadRequest(new { code = false, msg = "封面上传失败：" +ex.Message});
                }
            }

            var book = new Book
            {
                Name = request.Name,
                Author = request.Author,
                Price = request.Price,
                PublishDate = request.PublishDate,
                CategoryId = request.CategoryId,
                PublisherId = request.PublisherId,
                TotalCopies = request.TotalCopies,
                CoverUrl = coverUrl, //保存URL
                Status = 1, //默认启动 1.在库 2.下架
                IsDeleted = false,  //软删除
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            await _bookService.AddAsync(book);
            return Ok(new { code = true, msg = "图书添加成功", data = new { book.Id, book.CoverUrl} });
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
        public async Task<IActionResult> Update([FromForm] UpdateBookRequest request)
        {
            //获取当前图书信息
            var existingBook = await _bookService.GetByIdAsync(request.Id);
            if (existingBook == null)
                return NotFound(new { code = false, msg = "图书不存在"});

            if (request.Status == 2 && existingBook.Status != 2)
            {
                //检查是否有未归还的图书借阅信息
                bool hasUnreturnRecord = await _borrowService.HasUnreturnRecordAsync(request.Id);
                if (hasUnreturnRecord)
                {
                    return BadRequest(new { code = 400, msg = "该图书还有未归还的借阅记录，无法下架" });
                }
               
            }

            string? coverUrl = existingBook.CoverUrl; //默认保留原图
            if (request.CoverFile != null)
            {
                try
                {
                    coverUrl = await _ossService.UploadFileAsync(request.CoverFile, "book-covers");
                }
                catch (Exception ex)
                {
                    return BadRequest(new { code = false, msg = "封面图片上传失败" + ex.Message});
                }
            }

            existingBook.Name = request.Name;
            existingBook.Author = request.Author;
            existingBook.PublishDate = request.PublishDate;
            existingBook.Price = request.Price;
            existingBook.CategoryId = request.CategoryId;
            existingBook.PublisherId = request.PublisherId;
            existingBook.TotalCopies = request.TotalCopies;
            existingBook.Status = request.Status;
            existingBook.CoverUrl = coverUrl; //更新封面图片
            existingBook.UpdateTime = DateTime.Now;

            await _bookService.UpdateAsync(existingBook);
            return Ok(new { code = true, msg = "更新成功", data = new { existingBook.Id, existingBook.CoverUrl} });
        }

        /// <summary>
        /// 删除图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _bookService.DeleteAsync(id);
            return Ok(new { code = true, msg = "删除成功" });
        }

        /// <summary>
        /// 获取可借阅的图书（未删除且有库存）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Available([FromQuery] string keyword = "", [FromQuery] int page = 1, int pageSize = 20)
        {
            var result = await _bookService.GetAvailableBooksAsync(keyword, page, pageSize);
            return Ok(result);
        }
    }
}

