using LibraryManagement.DTO;

namespace LibraryManagement.Services
{
    public interface IReportService
    {
        /// <summary>
        /// 获取图书统计图表数据
        /// </summary>
        /// <returns></returns>
        Task<BookSummaryDto> GetBookSummaryAsync();

        /// <summary>
        /// 获取图书概览数据
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        Task<List<ChartDataDto>> GetBookStatsAsync(string dimension, int? startYear, int? endYear);

        /// <summary>
        /// 获取分类统计数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CategoryStatsResponseDto> GetCategoryStatsAsync(string type);

        /// <summary>
        /// 获取借阅统计数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BorrowStatsResponseDto> GetBorrowStatsAsync(BorrowStatsRequestDto request);
    }
}
