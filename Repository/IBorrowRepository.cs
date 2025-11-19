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
        Task<IQueryable<BorrowRecord>> GetQueryableAsync();
    }
}
