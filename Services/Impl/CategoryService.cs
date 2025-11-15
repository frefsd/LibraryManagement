using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
    public class CategoryService: ICategoryService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入
        public CategoryService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        //获取所有分类
        public async Task<List<Category>> AddAsync() =>
            await _applicationDbContext.Categories.ToListAsync();
    }
}
