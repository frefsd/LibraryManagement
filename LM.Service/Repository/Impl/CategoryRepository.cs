using LibraryManagement.LM.Common.AppDbContext;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LibraryManagement.LM.Service.Repository.Impl
{
    public class CategoryRepository: ICategoryRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入
        public CategoryRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>> GetAllAsync()
        {
            return await _applicationDbContext.Categories.ToListAsync();
        }

        public IQueryable<Category> GetListAsync(Expression<Func<Category,bool>>? predicate = null)
        {
            var query = _applicationDbContext.Categories.AsQueryable();
            if (predicate != null)
                query = query.Where(predicate);
            return query;
            
        }
        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> prediction)
        {
            return await _applicationDbContext.Categories.AnyAsync(prediction);
        }

        /// <summary>
        /// 根据id获取分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _applicationDbContext.Categories.FindAsync(id);
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddAsync([FromBody]Category category)
        {
            //添加分类到数据库
            _applicationDbContext.Categories.Add(category);
            //保存
            await _applicationDbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task UpdateAsync(Category category)
        {
            _applicationDbContext.Entry(category).State = EntityState.Modified;
            await _applicationDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            //通过id从数据库中查找分类
            var categroy = await _applicationDbContext.Categories.FindAsync(id);
            //判断：存在
            if (categroy != null)
            {
                //移除
                _applicationDbContext.Categories.Remove(categroy);
                //保存
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 获取分类的数量
        /// </summary>
        /// <returns></returns>
        public async Task<int> CountAsync()
        {
            return await _applicationDbContext.Categories.CountAsync();
        }

        public IQueryable<Category> GetQueryableAsync()
        {
            return _applicationDbContext.Categories.AsQueryable();
        }
    }
}
