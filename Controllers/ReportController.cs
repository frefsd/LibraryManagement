using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 图书信息统计
    /// </summary>
    [ApiController]
    [Route("report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportBookService;

        //依赖注入
        public ReportController(IReportService reportBookService)
        {
            _reportBookService = reportBookService;
        }

        /// <summary>
        /// 获取图书统计报表数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("book/summary")]
        public async Task<IActionResult> BookSummary()
        {
            var data = await _reportBookService.GetBookSummaryAsync();
            return Ok(new {code = true, data});
        }

        /// <summary>
        /// 获取图书概览数据
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        [HttpGet("book/stats")]
        public async Task<IActionResult> BookStats([FromQuery] string dimension, [FromQuery] int? startYear, [FromQuery] int? endYear)
        {
            var data = await _reportBookService.GetBookStatsAsync(dimension, startYear, endYear);
            return Ok(new {code = true, data});
        }
    }
}
