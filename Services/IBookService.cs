using LibraryManagement.Models;
using LibraryManagement.Result;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 图书管理接口实现
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
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
        Task UpdateAsync(Book book);

        /// <summary>
        /// 删除图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);


        /// <summary>
        /// 获取可借阅的图书（未删除且有库存）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PageResult<Book>> GetAvailableBooksAsync(string keyword, int page, int pageSize);

        /// <summary>
        /// ai关键词检索信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        Task<List<Book>> SearchBookByKeywordAsync(string keyword);
    }
}