using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 用于封装编辑图书返回的结果
    /// </summary>
    public class BookUpdateDto
    {
        public int Id { get; set; } //图书ID

        [Required] //必填项
        public string? Name { get; set; } //图书名称

        [Required] //必填项
        public string? Author { get; set; } //作者

        public DateTime PublishDate { get; set; } //发布日期

        public decimal Price { get; set; } //图书价格

        // 非空，必须选择分类
        [Range(1,int.MaxValue, ErrorMessage ="分类不能为空")]
        public int CategoryId { get; set; }

        // 非空：必须选择出版社
        [Range(1, int.MaxValue, ErrorMessage = "出版社不能为空")]
        public int PublisherId { get; set; }

        public int Status { get; set; } = 1; //1.在库 2.已借出 3.下架
    }
}
