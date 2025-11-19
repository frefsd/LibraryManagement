using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 数据信息统计报表
    /// </summary>
    [ApiController]
    [Route("report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        //依赖注入
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        ///  获取图书概览数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("book/summary")]
        public async Task<IActionResult> BookSummary()
        {
            var data = await _reportService.GetBookSummaryAsync();
            return Ok(new { code = true, data });
        }

        /// <summary>
        ///获取图书统计报表数据
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        [HttpGet("book/stats")]
        public async Task<IActionResult> BookStats([FromQuery] string dimension, [FromQuery] int? startYear, [FromQuery] int? endYear)
        {
            var data = await _reportService.GetBookStatsAsync(dimension, startYear, endYear);
            return Ok(new { code = true, data });
        }

        /// <summary>
        /// 获取分类统计数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("category/stats")]
        public async Task<IActionResult> CategoryStats([FromQuery] string type = "bookCount")
        {
            var data = await _reportService.GetCategoryStatsAsync(type);
            return Ok(new { code = true, data });
        }
    }
}
