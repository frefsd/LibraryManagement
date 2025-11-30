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
        /// 新增借阅
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task BorrowAsync(BorrowRequestDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.UserInput)) throw new DomainException("用户信息不能为空");

            //1.查询图书
            var book = await _bookRepository.GetByIdAsync(dto.BookId);
            //2.校验图书是否存在
            if (book == null) throw new DomainException("图书不存在");      
            //3.校验图书状态
            if (book.Status != 1) throw new DomainException("该图书已下架，无法借阅");
            //4.校验库存
            if (book.BorrowedCopies >= book.TotalCopies) throw new DomainException("该图书暂无库存");

            //根据ID或者用户搜索
            User? user;
            if (int.TryParse(dto.UserInput, out int userId))
            {
                user = await _userRepository.GetByIdAsync(userId);
            }
            else
            {
                user = await _userRepository.GetByUsernameAsync(dto.UserInput);
            }

            if (user == null) throw new DomainException("用户不存在，请检查输入的ID或用户名");
            if (user.Status != 1) throw new DomainException("该用户已被禁用");

            //获取该借阅人借阅的书籍数量
            var ActiveCount = await _borrowRepository.GetActiveBorrowCountAsync(userId);
            //判断
            if (ActiveCount >= 5) throw new DomainException("每位用户最多可借阅 5 本书，请先归还部分图书后再借阅");

            //检查该用户是否已借阅此书且未归还
            var existingRecord = await _borrowRepository.GetByUserIdAndBookIdAsync(userId, dto.BookId);

            //判断：如果记录存在，实际归还日期为空
            if (existingRecord != null && existingRecord.ActualReturnDate == null)
                throw new DomainException("您已借阅此图书，请先归还后再借阅");

            //执行借阅事务
            using var transaction = await _borrowRepository.BeginDbContextTransactionAsync();
            try
            {
                var record = new BorrowRecord
                {
                    BookId = dto.BookId,
                    UserId = user.Id,
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
        /// 归还图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ReturnAsync(int id)
        {
            var record = await _borrowRepository.GetByIdAsync(id);
            if (record == null || record.ActualReturnDate.HasValue)
                throw new DomainException("借阅记录不存在或已归还");

            var book = await _bookRepository.GetByIdAsync(record.BookId);
            if (book == null) throw new DomainException("关联图书不存在");

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
        /// 续借图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RenewAsync(int id)
        {
            //获取借阅信息
            var record = await _borrowRepository.GetByIdAsync(id);
            if (record == null) 
                throw new DomainException("借阅记录不存在");

            //已归还的不能续借
            if (record.ActualReturnDate.HasValue)
                throw new DomainException("该图书已归还，无法续借");

            //逾期的图书不能续借
            if (record.Status != 1)
                throw new DomainException("只用借阅中的图书才能续借");

            //限制续借次数
            if (record.RenewCount >= 1)
                throw new DomainException("该图书已续借，无法再次续借");

            //禁止逾期超过N天的续借
            if (DateTime.Now > record.DueDate.AddDays(7))
                throw new DomainException("逾期时间过长，无法续借");

            using var transaction = await _borrowRepository.BeginDbContextTransactionAsync();
            try
            {
                //延长应还日期
                record.DueDate = record.DueDate.AddDays(30);
                record.UpdateTime = DateTime.Now; //更新时间

                //判断该图书是否续借过
                if (record.RenewCount >= 1)
                    throw new DomainException("每本书仅允许续借一次");

                //记录续借次数
                record.RenewCount = (record.RenewCount ?? 0) + 1;

                await _borrowRepository.UpdateAsync(record);

                await _borrowRepository.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); //回滚
                throw;
            }
        }

        /// <summary>
        /// 检查指定用户是否存在未归还的借阅记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> HasActiveBorrowAsync(int userId)
        {
            return await _borrowRepository.HasActiveBorrowAsync(userId);
        }

        /// <summary>
        /// 检查指定图书是否存在未归还的借阅记录
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public async Task<bool> HasUnreturnRecordAsync(int bookId)
        {
            return await _borrowRepository.HasUnreturnRecordAsync(bookId);
        }

       
    }
}
