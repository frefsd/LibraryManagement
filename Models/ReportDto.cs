namespace LibraryManagement.Models
{
    /// <summary>
    /// 图书信息统计
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
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CategoryStatsItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public int BorrowCount { get; set; }
    }

    /// <summary>
    /// 分类信息统计
    /// </summary>
    public class CategorySummaryDto
    {
        public int TotalCategories { get; set; }
        public int EnabledCategories { get; set; }
        public string MaxBooksCategory { get; set; } = string.Empty;
        public decimal AvgBooksPerCategory { get; set; }
    }

    public class CategoryStatsResponseDto
    {
        public List<CategoryStatsItemDto> Stats { get; set; } = new();
        public CategorySummaryDto Summary { get; set; } = new();
    }


    public class BorrowStatsRequestDto
    {
        public string Dimension { get; set; } = "month";
        public int Limit { get; set; } = 10;
        public string? StartDate { get; set; } // "2025-06"
        public string? EndDate { get; set; }   // "2025-11"
    }

    public class BorrowStatItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }

    public class BorrowSummaryDto
    {
        public int TotalBorrows { get; set; }
        public int CurrentBorrowing { get; set; }
        public int OverdueCount { get; set; }
        public int AvgBorrowDays { get; set; }
    }

    public class BorrowStatsResponseDto
    {
        public List<BorrowStatItemDto> Stats { get; set; } = new();
        public BorrowSummaryDto Summary { get; set; } = new();
    }
}
