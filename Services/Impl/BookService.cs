using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 图书管理业务代码开发
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowRepository _borrowRepository;

        //依赖注入
        public BookService(IBookRepository bookRepository,
            IBorrowRepository borrowRepository)
        {
            _bookRepository = bookRepository;
            _borrowRepository = borrowRepository;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageResult<Book>> GetPageAsync(string? begin, string? end, string? name, int page = 1, int pageSize = 10)
        {
            var query = _bookRepository.GetQueryable();

            //按书名模糊查询
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            //按出版日期范围筛选
            if (DateTime.TryParse(begin, out var beginDate))
            {
                query = query.Where(b => b.PublishDate >= beginDate.Date);
            }
            if (DateTime.TryParse(end, out var endDate))
            {
                query = query.Where(b => b.PublishDate <= endDate.Date.AddDays(1).AddTicks(-1)); //包含整数 ;
            }

            var total = await query.CountAsync(); //查询总图书数量
            var skip = (page - 1) * pageSize;
            //每页展示的图书数量
            var rows = await query
                .OrderBy(b => b.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Book>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Rows = rows
            };
        }

        /// <summary>
        /// 添加图书
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task AddAsync(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Name))
                throw new DomainException("书名不能为空");

            //检查是否已存在同名同作者的有效图书
            var existingBook = await _bookRepository.GetQueryable()
                .FirstOrDefaultAsync(b =>
                b.Name == book.Name &&
                b.Author == book.Author &&
                b.IsDeleted == false);

            if (existingBook != null)
                throw new DomainException($"图书《{book.Name}》（作者：{book.Author}）已存在");

            book.CreateTime = DateTime.Now;
            book.UpdateTime = DateTime.Now;
            await _bookRepository.AddAsync(book);
        }

        /// <summary>
        /// 根据id获取图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// 修改图书信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task UpdateAsync(Book book)
        {
            var existingBook = await _bookRepository.GetByIdAsync(book.Id);
            if (existingBook == null)
                throw new DomainException($"图书ID {book.Id} 不存在");

            // 2. 如果用户尝试将状态从 1（在库）改为 2（下架）
            if (existingBook.Status == 1 && book.Status == 2)
            {
                // 3. 检查是否有未归还记录
                bool hasUnreturned = await _borrowRepository.HasUnreturnRecordAsync(book.Id);
                if (hasUnreturned)
                {
                    throw new DomainException("该图书还有未归还的借阅记录，无法下架");
                }
            }
            
            existingBook.PublishDate = book.PublishDate;
            existingBook.Price = book.Price;
            existingBook.CategoryId = book.CategoryId;
            existingBook.PublisherId = book.PublisherId;
            existingBook.Status = book.Status;
            existingBook.CoverUrl = book.CoverUrl;
            if (book.TotalCopies > 0)
            {
                existingBook.TotalCopies = book.TotalCopies;
            }
            existingBook.UpdateTime = DateTime.Now;

            await _bookRepository.UpdateAsync(existingBook); // ← 传已跟踪实体
        }

        /// <summary>
        /// 删除图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            //检查是否还有未归还的借阅记录
            var hasActiveBorrows = await _borrowRepository
                .GetQueryableAsync()
                .AnyAsync(r => r.BookId == id && r.Status == 1); //1.借阅中
            if (hasActiveBorrows)
            {
                throw new DomainException("该图书正在被借阅，无法删除！");
            }

            await _bookRepository.SoftDeleteAsync(id);
        }

        /// <summary>
        /// 获取可借阅的图书（未删除且有库存）
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageResult<Book>> GetAvailableBooksAsync(string keyword, int page, int pageSize)
        {
            return await _bookRepository.GetAvailableBooksAsync(keyword, page, pageSize);
        }
    }
}