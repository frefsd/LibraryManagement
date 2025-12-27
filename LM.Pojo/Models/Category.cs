using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.LM.Pojo.Models
{
    /// <summary>
    /// 图书分类表
    /// </summary>
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; } // 主键，自增ID

        public string? Name { get; set; } // 分类名称
        public int Sort { get; set; } // 排序序号
        public int Status { get; set; } = 1; // 状态：1-启用，0-禁用

        // 反向导航属性：该分类下的所有图书
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}