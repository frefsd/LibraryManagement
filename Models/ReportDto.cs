namespace LibraryManagement.Models
{
    public class BookSummaryDto
    {
        public int TotalBooks { get; set; } //图书总量
        public int AvailableBooks { get; set; } 
        public int BorrowedBooks { get; set; } //借出去的书籍
        public int CategoryCount { get; set; } //分类总量
    }

    public class ChartDataDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
