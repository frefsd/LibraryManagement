using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        //依赖注入
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
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
        public async Task UpdateAsync(Book book)
        {
            book.UpdateTime = DateTime.Now;
            await _bookRepository.UpdateAsync(book);
        }

        /// <summary>
        /// 删除图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
           await _bookRepository.DeleteAsync(id);
        }
    }
}