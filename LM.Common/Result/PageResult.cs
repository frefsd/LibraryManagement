namespace LibraryManagement.LM.Common.Result
{
    /// <summary>
    /// 分页查询返回的结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
        public int Total { get; set; } //总记录数
        public int Page { get; set; } //当前页码
        public int PageSize { get; set; } //每页大小
        public List<T> Rows { get; set; } = new(); //数据列表
    }
}