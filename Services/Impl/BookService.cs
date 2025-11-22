using LibraryManagement.DTO;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
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

            var total = await query.CountAsync();
            var skip = (page - 1) * pageSize;
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
        public async Task UpdateAsync(BookUpdateDto dto)
        {
            var existingBook = await _bookRepository.GetByIdAsync(dto.Id);
            if (existingBook == null)
                throw new KeyNotFoundException($"图书ID {dto.Id} 不存在");

            // 只更新标量字段，不碰导航属性！
            existingBook.Name = dto.Name;
            existingBook.Author = dto.Author;
            existingBook.PublishDate = dto.PublishDate;
            existingBook.Price = dto.Price;
            existingBook.CategoryId = dto.CategoryId;
            existingBook.PublisherId = dto.PublisherId;
            existingBook.Status = dto.Status;
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
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PageResult<Book>> GetAvailableBooksAsync(string keyword, int page, int pageSize)
        {
            return await _bookRepository.GetAvailableBooksAsync(keyword, page, pageSize);
        }
    }
}