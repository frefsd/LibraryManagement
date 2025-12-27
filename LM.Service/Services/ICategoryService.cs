using LibraryManagement.LM.Pojo.DTO;
using LibraryManagement.LM.Pojo.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.LM.Service.Services
{
    /// <summary>
    /// 分类管理接口实现
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        Task<List<Category>> GetAllAsync(); //用于下拉框

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(List<Category> Rows, int Total)> GetPageAsync(int page, int pageSize);

        /// <summary>
        /// 通过id获取分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddAsync([FromBody]CategoryDto dto);

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task UpdateAsync(int id, CategoryDto dto);

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
    }
}
