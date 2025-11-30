using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagement.Repository
{
    /// <summary>
    /// 借阅管理模块接口
    /// </summary>
    public interface IBorrowRepository
    {
        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(List<BorrowRecord> records, long total)> GetPageAsync(int page, int pageSize);
        /// <summary>
        /// 通过id获取借阅信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BorrowRecord?> GetByIdAsync(int id);

        /// <summary>
        /// 添加的借阅人信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(BorrowRecord entity);

        /// <summary>
        /// 修改借阅人信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(BorrowRecord entity);

        Task<IDbContextTransaction> BeginDbContextTransactionAsync();

        Task<int> SaveChangesAsync();

        /// <summary>
        /// 获取借阅信息
        /// </summary>
        /// <returns></returns>
        IQueryable<BorrowRecord> GetQueryableAsync();

        /// <summary>
        /// 检查图书是否正在被借阅（存在未归还记录）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasActiveBorrowAsync(int userId);

        /// <summary>
        /// 检查指定图书是否存在未归还的借阅记录
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        Task<bool> HasUnreturnRecordAsync(int bookId);

        /// <summary>
        /// 检查该借阅人借阅的书籍数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<int> GetActiveBorrowCountAsync(int userId);

        /// <summary>
        /// 检查该用户是否已借阅此书且未归还
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        Task<BorrowRecord?> GetByUserIdAndBookIdAsync(int userId, int bookId);
    }
}
