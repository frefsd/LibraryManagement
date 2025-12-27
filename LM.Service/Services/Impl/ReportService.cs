using LibraryManagement.LM.Pojo.DTO;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Repository;
using LibraryManagement.LM.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.LM.Service.Services.Impl
{
    /// <summary>
    /// 数据报表统计
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowRepository _borrowRepository;
        private readonly IUserRepository _userRepository;

        //依赖注入
        public ReportService(
            ICategoryRepository categoryRepository,
            IBookRepository bookRepository,
            IBorrowRepository borrowRepository,
            IUserRepository userRepository
            )
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
            _borrowRepository = borrowRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取图书统计报表数据
        /// </summary>
        /// <returns></returns>
        public async Task<BookSummaryDto> GetBookSummaryAsync()
        {
            //获取所有图书总量
            var totalBooks = await _bookRepository.GetTotalCopiesAsync();
            //借出的图书总量
            var borrowedBooks = await _bookRepository.GetBorrowedCopiesAsync();
            //图书馆剩余图书
            var availableBooks = totalBooks - borrowedBooks;
            //获取所有分类
            var categoryCount = await _categoryRepository.CountAsync();

            return new BookSummaryDto
            {
                TotalBooks = totalBooks,
                AvailableBooks = availableBooks,
                BorrowedBooks = borrowedBooks,
                CategoryCount = categoryCount
            };
        }

        /// <summary>
        /// 获取图书概览数据
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<List<ChartDataDto>> GetBookStatsAsync(string dimension, int? startYear = null, int? endYear = null)
        {
            var query = _bookRepository.GetQueryableAsync();

            //年份过滤
            if (dimension == "year" && startYear.HasValue && endYear.HasValue)
            {
                query = query.Where(b => b.PublishDate.Year >= startYear.Value && b.PublishDate.Year <= endYear.Value);
            }

            return dimension switch
            {
                "category" => await GetByCategory(query),
                "status" => await GetByStatus(query),
                "year" => await GetByYear(query),
                _ => throw new ArgumentException("不支持的统计维度")
            };
        }

        /// <summary>
        /// 通过分类统计图书报表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<List<ChartDataDto>> GetByCategory(IQueryable<Book> query)
        {
            //获取所有分类
            var categories = await _categoryRepository.GetAllAsync();
            //查询每个id所对应的分类
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            //按照CategoryId分组并统计总册数
            var dbResult = await query
                .GroupBy(b => b.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Sum(x => x.TotalCopies)
                }).ToListAsync(); //脱离EF，进入内存

            var result = dbResult
                 .Select(item =>
                 {
                     var categoryName = categoryMap.TryGetValue(item.CategoryId, out var name) ? name : $"分类{item.CategoryId}";

                     return new ChartDataDto
                     {
                         Name = categoryName,
                         Count = item.Count
                     };
                 }).ToList();

            return result;
        }

        /// <summary>
        /// 通过状态统计图书报表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<List<ChartDataDto>> GetByStatus(IQueryable<Book> query)
        {
            var statusMap = new Dictionary<int, string>
            {
                {1, "正常" },
                {2, "下架" },
                {3, "损坏" }
            };

            var rawData = await query
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Sum(x => x.TotalCopies) })
                .ToListAsync();

            return rawData.Select(x => new ChartDataDto
            {
                Name = statusMap.TryGetValue(x.Status, out var name) ? name : $"状{x.Status}",
                Count = x.Count
            }).ToList();
        }

        /// <summary>
        /// 通过图书出版年份统计图书报表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<ChartDataDto>> GetByYear(IQueryable<Book> query)
        {
            var data = await query
                .GroupBy(b => b.PublishDate.Year)
                .Select(g => new ChartDataDto
                {
                    Name = g.Key.ToString(),
                    Count = g.Sum(x => x.TotalCopies)
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return data;
        }

        /// <summary>
        /// 图书分类统计
        /// </summary>
        /// <returns></returns>
        public async Task<CategoryStatsResponseDto> GetCategoryStatsAsync(string type)
        {
            //1.获取所有分类
            var allCategories = await _categoryRepository.GetAllAsync();
            var categoryDict = allCategories.ToDictionary(c => c.Id, c => c.Name);


            //2.查询每个分类的图书总册数
            //2.1：先获取IQueryable<book>
            var booksQuery = _bookRepository.GetQueryableAsync();
            //2.2:从IQueryable<book>中获取GroupBy（）方法
            var bookCounts = await booksQuery
                .GroupBy(b => b.CategoryId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(x => x.TotalCopies));
 

            //3.查询每个分类的借阅总次数
            var borrowCounts = await (_borrowRepository.GetQueryableAsync())
                .Where(r => r.Status == 1)
                .Join(
                booksQuery, //关联book表
                record => record.BookId, //BorrowRecord.BookId
                book => book.Id, //Book.Id
                (record, book) => book.CategoryId
                )
                .GroupBy(categoryId => categoryId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            //4.构建stats列表（包含所有分类）
            var stats = allCategories.Select(category =>
            {
                var bookCount = bookCounts.TryGetValue(category.Id, out var bc) ? bc : 0;
                var borrowCount = borrowCounts.TryGetValue(category.Id, out var br) ? br : 0;

                return new CategoryStatsItemDto
                {
                    Name = category.Name,
                    BookCount = bookCount,
                    BorrowCount = borrowCount
                };
            }).ToList();

            //5.计算summary
            var totalBookCopies = stats.Sum(s => s.BookCount);
            var maxCategory = stats.OrderByDescending(s => s.BookCount).FirstOrDefault();

            var summary = new CategorySummaryDto
            {
                TotalCategories = allCategories.Count,
                EnabledCategories = allCategories.Count(c => c.Status == 1),
                MaxBooksCategory = maxCategory?.Name ?? "无",
                AvgBooksPerCategory = allCategories.Count > 0
                ? Math.Round((decimal)totalBookCopies / allCategories.Count, 1) : 0
            };

            return new CategoryStatsResponseDto
            {
                Stats = stats,
                Summary = summary
            };
        }

        /// <summary>
        /// 借阅信息统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BorrowStatsResponseDto> GetBorrowStatsAsync(BorrowStatsRequestDto request)
        {
            var borrowQuery = _borrowRepository.GetQueryableAsync();
            var summary = await GetBorrowSummaryAsync(borrowQuery);

            var stats = request.Dimension.ToLower() switch
            {
                "month" => await GetMonthBorrowStatsAsync(borrowQuery, request.StartDate, request.EndDate),
                "book" => await GetTopBooksBorrowStatsAsync(borrowQuery, request.Limit),
                "user" => await GetTopUsersBorrowStatsAsync(borrowQuery, request.Limit),
                "category" => await GetTopCategoriesBorrowStatsAsync(borrowQuery, request.Limit),
                _ => await GetMonthBorrowStatsAsync(borrowQuery, request.StartDate, request.EndDate)
            };

            return new BorrowStatsResponseDto
            {
                Stats = stats,
                Summary = summary
            };
        }

        /// <summary>
        /// 按分类统计借阅信息
        /// </summary>
        /// <param name="borrowQuery"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<BorrowStatItemDto>> GetTopCategoriesBorrowStatsAsync(IQueryable<BorrowRecord> borrowQuery, int limit)
        {
            var bookQuery = _bookRepository.GetQueryableAsync();
            var categoryQuery = _categoryRepository.GetQueryableAsync();

            var result = await borrowQuery
                .Join(bookQuery, br => br.BookId, b => b.Id, (br, b) => new { b.CategoryId })
                .Join(categoryQuery, x => x.CategoryId, c => c.Id, (x, c) => new { CategoryName = c.Name ?? "未分类" })
                .GroupBy(x => x.CategoryName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync();

            return result.Select(x => new BorrowStatItemDto
            {
                Name = x.Name,
                BorrowCount = x.Count
            }).ToList();
        }

        /// <summary>
        /// 按借阅用户统计借阅信息
        /// </summary>
        /// <param name="borrowQuery"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<BorrowStatItemDto>> GetTopUsersBorrowStatsAsync(IQueryable<BorrowRecord> borrowQuery, int limit)
        {
            var useQuery = _userRepository.GetQueryableAsync();

            var result = await borrowQuery
                .Join(useQuery,
                b => b.UserId,
                u => u.Id,
                (b, u) => new { UserName = u.Name ?? "匿名用户" })
                .GroupBy(x => x.UserName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync();

            return result.Select(x => new BorrowStatItemDto
            {
                Name = x.Name,
                BorrowCount = x.Count
            }).ToList();
        }

        /// <summary>
        /// 按借阅的图书统计借阅信息
        /// </summary>
        /// <param name="borrowQuery"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<BorrowStatItemDto>> GetTopBooksBorrowStatsAsync(IQueryable<BorrowRecord> borrowQuery, int limit)
        {
            var bookQuery = _bookRepository.GetQueryableAsync();

            var result = await borrowQuery
                .Join(bookQuery,
                b => b.BookId,
                book => book.Id,
                (b, book) => new { BookName = book.Name ?? "未知图书" })
                .GroupBy(x => x.BookName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync();

            return result.Select(x => new BorrowStatItemDto
            {
                Name = x.Name,
                BorrowCount = x.Count

            }).ToList();
        }

        /// <summary>
        /// 按月统计借阅信息
        /// </summary>
        /// <param name="query"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<BorrowStatItemDto>> GetMonthBorrowStatsAsync(IQueryable<BorrowRecord> query, string? start, string? end)
        {
            DateTime startDate = string.IsNullOrEmpty(start)
                ? DateTime.Now.AddMonths(-5).Date
                : DateTime.Parse(start + "-01");
            DateTime endDate = string.IsNullOrEmpty(end)
                ? DateTime.Now.Date
                : DateTime.Parse(end + "-01").AddMonths(1).AddDays(-1);

            //1.先从数据库中查询
            var rawData = await query
                .Where(r => r.BorrowDate >= startDate && r.BorrowDate <= endDate)
                .GroupBy(r => new { r.BorrowDate.Year, r.BorrowDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                }).ToListAsync(); //结束数据库查询

            //2.在内存中格式化Name并排序
            var result = rawData
                .Select(x => new BorrowStatItemDto
                {
                    Name = $"{x.Year}-{x.Month:D2}", //在内存中执行
                    BorrowCount = x.Count
                })
                .OrderBy(X => X.Name) //进行排序
                .ToList();

            return result;
        }

        /// <summary>
        /// 借阅信息统计
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<BorrowSummaryDto> GetBorrowSummaryAsync(IQueryable<BorrowRecord> query)
        {
            var total = await query.CountAsync();
            var current = await query.CountAsync(r => r.Status == 1); //1.借阅中
            var overdue = await query.CountAsync(r => r.Status == 3); //3.逾期

            var returnedRecords = await query
                .Where(r => r.Status == 2 && r.ActualReturnDate.HasValue)
                .Select(r => new { r.BorrowDate, r.ActualReturnDate })
                .ToListAsync();

            //计算平均借阅天数
            double avgDays = returnedRecords.Any()
                ? returnedRecords.Average(r => (r.ActualReturnDate.Value - r.BorrowDate).TotalDays)
                : 0.0;

            return new BorrowSummaryDto
            {
                TotalBorrows = total,
                CurrentBorrowing = current,
                OverdueCount = overdue,
                AvgBorrowDays = (int)Math.Round(avgDays)
            };
        }
    }
}
