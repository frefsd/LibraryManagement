using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.LM.Pojo.DTO
{
    /// <summary>
    /// 新增图书请求参数
    /// </summary>
    public class CreateBookRequest
    {
        [Required(ErrorMessage = "书名不能为空")]
        public string? Name { get; set; } //图书名称

        [Required(ErrorMessage = "作者不能为空")]
        public string? Author { get; set; } //图书作者

        public DateTime PublishDate { get; set; } //出版日期

        public decimal Price { get; set; } // 图书价格

        [Range(1, int.MaxValue, ErrorMessage = "分类不能为空")]
        public int CategoryId { get; set; } // 所属分类

        [Range(1, int.MaxValue, ErrorMessage = "出版社不能为空")]
        public int PublisherId { get; set; } //所属出版社

        public int TotalCopies { get; set; } = 1; // 图书总量默认为1

        public IFormFile? CoverFile { get; set; } // 封面文件
    }
}
