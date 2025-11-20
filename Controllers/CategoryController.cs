using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 图书分类模块
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        //依赖注入
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(new { code = true, data = categories });
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
            var (rows, total) = await _categoryService.GetPageAsync(page, pageSize);
            return Ok(new { code = true, data = new { rows, total } });
        }

        /// <summary>
        /// 根据id获取分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> QueryInfo(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            //判读：分类是否存在
            if (category == null)
            {
                return NotFound(new { code = false, msg = "分类不存在" });
            }

            return Ok(new { code = true, data = category });
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CategoryDto dto)
        {

            await _categoryService.AddAsync(dto);
            return Ok(new { code = true, msg = "添加成功" });
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(new { code = true, msg = "删除成功" });
        }

        /// <summary>
        /// 修改分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
        {         
                await _categoryService.UpdateAsync(id, dto);
                return Ok(new { code = true, msg = "修改成功" });                     
        }
    }
}
