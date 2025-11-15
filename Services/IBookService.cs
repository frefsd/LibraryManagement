using LibraryManagement.Models;
using LibraryManagement.Result;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Services
{
    public interface IBookService
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageResult<Book>> GetPageAsync(string? begin, string? end, string? name, int page = 1, int pageSize = 10);

        /// <summary>
        /// 添加图书
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        Task AddAsync(Book book);

        /// <summary>
        /// 根据id来获取图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Book?> GetByIdAsync(int id);

        /// <summary>
        /// 修改图书信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        Task UpdateAsync([FromBody]Book book);

        /// <summary>
        /// 删除图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
    }
}