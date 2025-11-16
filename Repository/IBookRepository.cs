using LibraryManagement.Models;

namespace LibraryManagement.Repository
{
    public interface IBookRepository
    {
        /// <summary>
        /// 获取可查询的图书集合（包括分类和出版社）
        /// </summary>
        /// <returns></returns>
        IQueryable<Book> GetQueryable();

        /// <summary>
        /// 根据id获取图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Book?> GetByIdAsync(int id);

        /// <summary>
        /// 添加图书信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        Task AddAsync(Book book);

        /// <summary>
        /// 更新图书信息
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
        /// 判断该分类下是否有书籍
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> HasBooksByCategoryIdAsync(int categoryId);

    }
}