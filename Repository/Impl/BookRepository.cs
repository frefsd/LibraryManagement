using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using LibraryManagement.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repository.Impl
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;

        //依赖注入
        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取所有图书信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<Book> GetQueryable()
        {
            return _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .AsNoTracking(); //只读查询，提升性能
        }

        /// <summary>
        /// 根据id获取图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Book?> GetByIdAsync(int id)
        {
            //从GetQueryable()方法中得到要查询的数据信息
            return await GetQueryable().FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        ///添加图书
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync(); //保存到数据库中
        }

        /// <summary>
        /// 更新图书信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task UpdateAsync([FromBody] Book book)
        {
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除图书信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            //判断
            if (book != null)
            {
                //如果存在，删除
                _context.Books.Remove(book);
                //保存
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 查询已归还的图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task SoftDeleteAsync(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            book.IsDeleted = true;
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// 判断该分类下是否有书籍
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<bool> HasBooksByCategoryIdAsync(int categoryId)
        {
            return await _context.Books.AnyAsync(b => b.CategoryId == categoryId);
        }

        /// <summary>
        /// 查询所有书籍不包括（已下架）
        /// </summary>
        /// <returns></returns>
        public IQueryable<Book> GetQueryableAsync()
        {
            return _context.Books;
        }

        /// <summary>
        /// 获取图书的总数量（不包括已下架，损坏等）
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetTotalCopiesAsync()
        {
            return await _context.Books.SumAsync(b => b.TotalCopies);
        }

        /// <summary>
        /// 借阅出去的书籍总数量
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetBorrowedCopiesAsync()
        {
            return await _context.Books.SumAsync(b => b.BorrowedCopies);
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
            //从数据库中获取没有没软删除的图书
            var query = _context.Books
                .Where(b => !b.IsDeleted && b.TotalCopies > b.BorrowedCopies);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(b =>
                b.Name.Contains(keyword) || b.Author.Contains(keyword));
            }

            var total = await query.CountAsync();
            var books = await query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Book>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Rows = books
            };
        }
    }
}