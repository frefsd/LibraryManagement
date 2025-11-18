namespace LibraryManagement.Models
{
    /// <summary>
    /// 用户封装用户借阅记录返回的结果
    /// </summary>
    public class BorrowRecordDto
    {
        public int Id { get; set; } //主键
        public int BookId { get; set; } // 关联的图书ID
        public string BookName { get; set; } = string.Empty; //图书名称
        public int UserId { get; set; } //关联的用户ID
        public string UserName { get; set; } = string.Empty; //用户名
        public DateTime BorrowDate { get; set; } //借阅日期
        public DateTime DueDate { get; set; } //归还日期
        public DateTime? ActualReturnDate { get; set; } //实际归还日期（null == 未归还）
        public int Status { get; set; } // 1-借阅中，2-已归还，3-逾期
        //判读借阅人是在借阅中，已归还或则逾期
        public bool IsOverdue => ActualReturnDate == null && DateTime.Now > DueDate;
    }
}
