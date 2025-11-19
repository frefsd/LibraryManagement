using LibraryManagement.Models;
using LibraryManagement.Repository;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;

namespace LibraryManagement.Services.Impl
{
    public class ReportService : IReportService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;

        //依赖注入
        public ReportService(
            ICategoryRepository categoryRepository,
            IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
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
            var query = await _bookRepository.GetQueryableAsync();

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
    }
}
