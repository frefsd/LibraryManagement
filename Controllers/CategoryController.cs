using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 图书分类类
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class CategoryController: ControllerBase
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
        public async Task<IActionResult> All()
        {
            var categories = await _categoryService.AddAsync();
            return Ok(new { code = true, data = categories});
        }
    }
}
