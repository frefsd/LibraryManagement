using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 图书分类类
    /// </summary>
    public class CategoryController: ControllerBase
    {
        private readonly ICategoryService _categoryService;

        //依赖注入
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
    }
}
