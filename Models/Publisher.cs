using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 出版商类
    /// </summary>
    [Table("Publisher")]
    public class Publisher
    {
        [Key]
        public int Id { get; set; } // 主键，自增ID

        public string? Name { get; set; } // 出版商名称
        public string? Address { get; set; } // 地址
        public string? Phone { get; set; } // 联系电话
        public int Status { get; set; } = 1; // 状态：1-正常，0-停用

        // 反向导航属性：该出版商出版的所有图书
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}