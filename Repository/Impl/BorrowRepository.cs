using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagement.Repository.Impl
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
        /// <returns></returns>
        public async Task<(List<BorrowRecord> records, long total)> GetPageAsync(int page, int pageSize)
        {
            var query = _applicationDbContext.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreateTime);

            var total = await query.CountAsync();
            var records = await query
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
            //await _applicationDbContext.SaveChangesAsync();
        }
        
        public async Task<IDbContextTransaction> BeginDbContextTransactionAsync()
        {
            return await _applicationDbContext.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _applicationDbContext.SaveChangesAsync();
        }

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
    }
}
