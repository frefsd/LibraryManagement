namespace LibraryManagement.DTO
{
    /// <summary>
    /// 用户前端发起借书请求时传参
    /// </summary>
    public class BorrowRequestDto
    {
            public int BookId { get; set; }
            public string? UserInput { get; set; }
    }
}
