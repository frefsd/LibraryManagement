using LibraryManagement.DTO;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 借阅管理业务逻辑实现
    /// </summary>
    public class BorrowService : IBorrowService
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
            //1.查询图书和用户
            var book = await _bookRepository.GetByIdAsync(dto.BookId);
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            //2.校验图书是否存在
            if (book == null) throw new DomainException("图书不存在");
            //3.校验用户是否存在
            if (user == null) throw new DomainException("用户不存在");
            //3.校验图书状态
            if (book.Status != 1) throw new DomainException("该图书已下架，无法借阅");
            //4.校验用户状态
            if (user.Status != 1) throw new DomainException("改用户已被禁用");
            //5.校验库存
            if (book.BorrowedCopies >= book.TotalCopies) throw new DomainException("该图书暂无库存");

            //执行事务
            using var transaction = await _borrowRepository.BeginDbContextTransactionAsync();
            try
            {
                var record = new BorrowRecord
                {
                    BookId = dto.BookId,
                    UserId = dto.UserId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    Status = 1, //借阅中
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _borrowRepository.AddAsync(record);
                book.BorrowedCopies++; //增加已借数量
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

        /// <summary>
        /// 检查图书是否正在被借阅（存在未归还记录）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> HasActiveBorrowAsync(int userId)
        {
            return await _borrowRepository.HasActiveBorrowAsync(userId);
        }
    }
}
