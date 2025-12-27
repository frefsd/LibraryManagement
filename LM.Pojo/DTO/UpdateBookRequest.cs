using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.LM.Pojo.DTO
{
    public class UpdateBookRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "书名不能为空")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "作者不能为空")]
        public string Author { get; set; } = null!;

        public DateTime PublishDate { get; set; }

        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "分类不能为空")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "出版社不能为空")]
        public int PublisherId { get; set; }

        public int TotalCopies { get; set; }

        public int Status { get; set; } = 1;

        public IFormFile? CoverFile { get; set; } // ← 支持更换封面
    }
}
