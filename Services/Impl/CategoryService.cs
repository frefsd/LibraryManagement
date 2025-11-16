using LibraryManagement.Models;
using LibraryManagement.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
    public class CategoryService: ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;

        //依赖注入
        public CategoryService(ICategoryRepository categoryRepository, IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
        }

        /// <summary>
        /// 获取启用的分类
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>> GetAllAsync()
        {
            //只返回启用的分类
            return await _categoryRepository.GetListAsync(c => c.Status == 1)
                .OrderBy(c => c.Sort)
                .ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(List<Category> Rows, int Total)> GetPageAsync(int page, int pageSize)
        {
            var query = _categoryRepository.GetListAsync(c => true);
            var total = await query.CountAsync();
            var rows = await query
                .OrderBy(c => c.Sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (rows, total);
        } 

        /// <summary>
        /// 根据id获取分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddAsync([FromBody]CategoryDto dto)
        {
            //判断
            if (string.IsNullOrWhiteSpace(dto.Name))         
                throw new ArgumentException("分类名称不能为空");

            //判断：如果添加的分类与数据库中的分类相同
            var exists = await _categoryRepository.AnyAsync(c => c.Name == dto.Name);
            if (exists)          
                throw new InvalidOperationException("分类名称已存在");
            
            var category = new Category
            {
                Name = dto.Name.Trim(), //防止空格
                Sort = dto.Sort,
                Status = dto.Status
            };
            await _categoryRepository.AddAsync(category);
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task UpdateAsync(int id, CategoryDto dto)
        {
            //从GetByIdAsync()方法中获取id
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new ArgumentException("分类不存在");
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("分类名称不能为空");

            var nameExists = await _categoryRepository.AnyAsync(c => c.Name == dto.Name && c.Id != id);
            if (nameExists)
                throw new InvalidOperationException("该分类名称已被警用");

            category.Name = dto.Name.Trim();
            category.Sort = dto.Sort;
            category.Status = dto.Status;

            await _categoryRepository.UpdateAsync(category);
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            var hasBooks = await _bookRepository.HasBooksByCategoryIdAsync(id);
            if (hasBooks)
                throw new InvalidOperationException("该分类下存在图书，无法删除");

            await _categoryRepository.DeleteAsync(id);
        }

        /// <summary>
        /// 判断该分类下是否有书籍
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<bool> HasBooksAsync(int categoryId)
        {
            return await _bookRepository.HasBooksByCategoryIdAsync(categoryId);
        }
    }
}
