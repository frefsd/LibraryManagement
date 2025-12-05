namespace LibraryManagement.DTO
{
    /// <summary>
    /// 图书信息统计概览数据
    /// </summary>
    public class BookSummaryDto
    {
        public int TotalBooks { get; set; } //图书总量
        public int AvailableBooks { get; set; } //仓库中剩余书籍
        public int BorrowedBooks { get; set; } //借出去的书籍
        public int CategoryCount { get; set; } //分类总量
    }

    /// <summary>
    /// 图书信息统计饼状图
    /// </summary>
    public class ChartDataDto
    {
        public string? Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// 按分类统计图书数量
    /// </summary>
    public class CategoryStatsItemDto
    {
        public string? Name { get; set; } //分类名称
        public int BookCount { get; set; } //图书数量
        public int BorrowCount { get; set; } //借阅数量
    }

    /// <summary>
    /// 分类信息统计
    /// </summary>
    public class CategorySummaryDto
    {
        public int TotalCategories { get; set; } //总分类数
        public int EnabledCategories { get; set; } //启用的分类数
        public string? MaxBooksCategory { get; set; } //图书最多的分类
        public decimal AvgBooksPerCategory { get; set; } //平均图书数
    }

    public class CategoryStatsResponseDto
    {
        public List<CategoryStatsItemDto> Stats { get; set; } = new();
        public CategorySummaryDto Summary { get; set; } = new();
    }

    /// <summary>
    /// 借阅信息统计
    /// </summary>
    public class BorrowStatsRequestDto
    {
        public string Dimension { get; set; } = "month"; //按月统计借阅信息
        public int Limit { get; set; } = 10; //默认展示10条数据
        public string? StartDate { get; set; } // "2025-06" //开始时间
        public string? EndDate { get; set; }   // "2025-11" //结束时间
    }

    public class BorrowStatItemDto
    {
        public string? Name { get; set; }
        public int BorrowCount { get; set; }
    }

    public class BorrowSummaryDto
    {
        public int TotalBorrows { get; set; } //总借阅次数
        public int CurrentBorrowing { get; set; } //当前借阅中
        public int OverdueCount { get; set; } //逾期数量
        public int AvgBorrowDays { get; set; } //平均借阅时长
    }

    public class BorrowStatsResponseDto
    {
        public List<BorrowStatItemDto> Stats { get; set; } = new();
        public BorrowSummaryDto Summary { get; set; } = new();
    }
}
