using LibraryManagement.Models;
using LibraryManagement.Result;

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
        /// 软删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task SoftDeleteAsync(int id);

        /// <summary>
        /// 判断该分类下是否有书籍
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> HasBooksByCategoryIdAsync(int categoryId);

        /// <summary>
        /// 获取图书的总数量（不包括已下架，损坏等）
        /// </summary>
        /// <returns></returns> 
        Task<int> GetTotalCopiesAsync();

        /// <summary>
        /// 借阅出去的书籍总数量
        /// </summary>
        /// <returns></returns>
        Task<int> GetBorrowedCopiesAsync();

        /// <summary>
        ///查询所有书籍不包括（已下架）
        /// </summary>
        /// <returns></returns>
        IQueryable<Book> GetQueryableAsync();


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
        Task<List<Book>> GetBooksByKeywordAsync(string keyword);
    }
}