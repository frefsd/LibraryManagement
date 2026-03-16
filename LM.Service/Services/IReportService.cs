using LibraryManagement.LM.Pojo.dto;
using LibraryManagement.LM.Pojo.DTO;
using LibraryManagement.LM.Pojo.vo;

namespace LibraryManagement.LM.Service.Services
{
    /// <summary>
    /// 数据报表统计接口
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// 获取图书统计图表数据
        /// </summary>
        /// <returns></returns>
        Task<BookSummaryDTO> GetBookSummaryAsync();

        /// <summary>
        /// 获取图书概览数据
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        Task<List<ChartDataDTO>> GetBookStatsAsync(string dimension, int? startYear, int? endYear);

        /// <summary>
        /// 获取分类统计数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CategoryStatsVO> GetCategoryStatsAsync(string type);

        /// <summary>
        /// 获取借阅统计数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BorrowStatsVO> GetBorrowStatsAsync(BorrowStatsRequestDTO request);
    }
}
