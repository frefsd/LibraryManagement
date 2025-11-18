using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 借阅管理业务逻辑实现
    /// </summary>
    public class BorrowService:IBorrowService
    {
        private readonly IBorrowRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;

        //依赖注入
        public BorrowService(
            IBorrowRepository borrowRepository, 
            IBookRepository bookRepository, 
            IUserRepository userRepository)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageResult<BorrowRecordDto>> GetPageAsync(int page, int pageSize)
        {
            var (records, total) = await _borrowRepository.GetPageAsync(page, pageSize);
            var dtos = records.Select(r => new BorrowRecordDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookName = r.Book?.Name ?? "未知图书",
                UserId = r.UserId,
                UserName = r.User?.Name ?? "未知用户",
                BorrowDate = r.BorrowDate,
                DueDate = r.DueDate,
                ActualReturnDate = r.ActualReturnDate,
                Status = r.Status
            }).ToList();

            return new PageResult<BorrowRecordDto>
            {
                Total = (int)total,
                Page = page,
                PageSize = pageSize,
                Rows = dtos
            };
        }

        /// <summary>
        /// 获取借阅信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task BorrowAsync(BorrowRequestDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(dto.BookId);
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (book == null) throw new InvalidOperationException("图书不存在");
            if (user == null) throw new InvalidOperationException("用户不存在");
            if (book.Status != 1) throw new InvalidOperationException("该图书已下架");
            if (user.Status != 1) throw new InvalidOperationException("改用户已被禁用");
            if (book.BorrowedCopies >= book.TotalCopies) throw new InvalidOperationException("该图书暂无库存");

            using var transaction = await _borrowRepository.BeginDbContextTransactionAsync();
            try
            {
                var record = new BorrowRecord
                {
                    BookId = dto.BookId,
                    UserId = dto.UserId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    Status = 1,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _borrowRepository.AddAsync(record);
                book.BorrowedCopies++;
                await _bookRepository.UpdateAsync(book);

                await _borrowRepository.SaveChangesAsync();

                await transaction.CommitAsync(); //提交事务
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); //回滚
                throw;
            }
        }

        /// <summary>
        /// 获取图书归还信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ReturnAsync(int id)
        {
            var record = await _borrowRepository.GetByIdAsync(id);
            if (record == null || record.ActualReturnDate.HasValue)
                throw new InvalidOperationException("借阅记录不存在或已归还");

            var book = await _bookRepository.GetByIdAsync(record.BookId);
            if (book == null) throw new InvalidOperationException("关联图书不存在");

            using var transaction = await _borrowRepository.BeginDbContextTransactionAsync();
            try
            {
                record.ActualReturnDate = DateTime.Now;
                record.UpdateTime = DateTime.Now;
                record.Status = DateTime.Now > record.DueDate ? 3 : 2;

                if (book.BorrowedCopies > 0)
                {
                    book.BorrowedCopies--;
                    await _bookRepository.UpdateAsync(book);
                }

                await _borrowRepository.UpdateAsync(record);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
