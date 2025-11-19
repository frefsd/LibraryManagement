using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace LibraryManagement.Repository
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        Task<List<Category>> GetAllAsync();

        IQueryable<Category> GetListAsync(Expression<Func<Category, bool>>? predicate = null);

        Task<bool> AnyAsync(Expression<Func<Category, bool>> prediction);
        /// <summary>
        /// 根据id获取分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddAsync([FromBody]Category category);

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task UpdateAsync(Category category);

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
        Task<int> CountAsync();
    }
}
