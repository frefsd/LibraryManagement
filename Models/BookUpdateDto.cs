using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class BookUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Author { get; set; }

        public DateTime PublishDate { get; set; }

        public decimal Price { get; set; }

        // 非空，必须选择分类
        [Range(1,int.MaxValue, ErrorMessage ="分类不能为空")]
        public int CategoryId { get; set; }

        // 非空：必须选择出版社
        [Range(1, int.MaxValue, ErrorMessage = "出版社不能为空")]
        public int PublisherId { get; set; }

        public int Status { get; set; } = 1; //1.在库 2.已借出 3.下架
    }
}
