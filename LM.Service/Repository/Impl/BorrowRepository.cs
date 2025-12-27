using LibraryManagement.LM.Common.AppDbContext;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Dynamic.Core;

namespace LibraryManagement.LM.Service.Repository.Impl
{

    public class BorrowRepository : IBorrowRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入 
        public BorrowRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="userName"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<(List<BorrowRecord> records, long total)> GetPageAsync(int page, int pageSize, string? userName, int? status)
        {
            var query = _applicationDbContext.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .AsQueryable();

            //按借阅人姓名模糊查询
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(r => r.User.Name.Contains(userName));
            }

            var now = DateTime.Now; //获取现在的时间
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case 1: // 借阅中（未归还 + 未逾期）
                        query = query.Where(r => r.ActualReturnDate == null && r.DueDate >= now);
                        break;
                    case 2: // 已归还
                        query = query.Where(r => r.ActualReturnDate != null);
                        break; // 借阅中（未归还 + 逾期）
                    case 3:
                        query = query.Where(r => r.ActualReturnDate == null && r.DueDate < now);
                        break;
                }
            }
            

            var total = await query.CountAsync();
            var records = await query
                .OrderByDescending(r => r.BorrowDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (records, total);
        }

        /// <summary>
        /// 通过id获取借阅人信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BorrowRecord?> GetByIdAsync(int id)
        {
            return await _applicationDbContext.BorrowRecords.FindAsync(id);
        }

        /// <summary>
        /// 添加借阅人信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddAsync(BorrowRecord entity)
        {
            await _applicationDbContext.BorrowRecords.AddAsync(entity);
            //await _applicationDbContext.SaveChangesAsync();

        }

        /// <summary>
        /// 修改借阅人信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task UpdateAsync(BorrowRecord entity)
        {
            _applicationDbContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
        
        public async Task<IDbContextTransaction> BeginDbContextTransactionAsync()
        {
            return await _applicationDbContext.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _applicationDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取借阅信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<BorrowRecord> GetQueryableAsync()
        {
            return _applicationDbContext.BorrowRecords;
        }

        /// <summary>
        /// 检查图书是否正在被借阅（存在未归还记录）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> HasActiveBorrowAsync(int userId)
        {
            return await _applicationDbContext.BorrowRecords.AnyAsync(br => br.UserId == userId && br.ActualReturnDate == null);
        }

        /// <summary>
        /// 检查指定图书是否存在未归还的借阅记录
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public async Task<bool> HasUnreturnRecordAsync(int bookId)
        {
            return await _applicationDbContext.BorrowRecords.AnyAsync(br => br.BookId == bookId && br.Status == 1); //1.借阅中
        }

        /// <summary>
        /// 检查该借阅人借阅书籍的数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> GetActiveBorrowCountAsync(int userId)
        {
            return await _applicationDbContext.BorrowRecords
                .CountAsync(r => r.UserId == userId && r.ActualReturnDate == null); //ActualReturnDate == null 表示”尚未归还“ 正在借阅中
        }

        /// <summary>
        /// 检查该用户是否已借阅此书且未归还
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public async Task<BorrowRecord?> GetByUserIdAndBookIdAsync(int userId, int bookId)
        {
            return await _applicationDbContext.BorrowRecords
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId && r.ActualReturnDate == null);
        }
    }
}
