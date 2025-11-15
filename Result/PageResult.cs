namespace LibraryManagement.Result
{
    /// <summary>
    /// 分页查询返回的结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Rows { get; set; } = new();
    }
}