namespace LibraryManagement.LM.Pojo.dto
{
    /// <summary>
    /// 用于封装分类返回的结果
    /// </summary>
    public class CategoryDTO
    {
        public string? Name { get; set; } //分类名称
        public int Sort { get; set; } //排序序号
        public int Status { get; set; } = 1; //1.启用 0.禁止

    }
}
