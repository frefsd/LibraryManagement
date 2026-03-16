namespace LibraryManagement.LM.Pojo.dto

{
    /// <summary>
    /// 新增借阅请求参数
    /// </summary>
    public class BorrowRequestDTO
    {
            public int BookId { get; set; } //图书ID
            public string? UserInput { get; set; } //用户输入用户名或者用户ID
    }
}
