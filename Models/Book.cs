using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 图书表
    /// </summary>
    [Table("Book")]
    public class Book
    {
        [Key]
        public int Id { get; set; } // 主键，自增ID

        public string? Name { get; set; } // 书名
        public string? Author { get; set; } // 作者姓名
        public DateTime PublishDate { get; set; } // 出版日期
        public decimal Price { get; set; } // 图书价格（单位：元）

        // 外键：关联分类表 Category 的主键 Id
        public int CategoryId { get; set; }

        // 导航属性：指向所属的图书分类
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // 外键：关联出版商表 Publisher 的主键 Id
        public int PublisherId { get; set; }

        // 导航属性：指向出版商信息
        [ForeignKey("PublisherId")]
        public virtual Publisher? Publisher { get; set; }

        public int TotalCopies { get; set; } = 1; //总册数

        public int BorrowedCopies { get; set; } = 0; // 已借出册数

        public int Status { get; set; } = 1; // 1-正常，2-已下架

        public DateTime UpdateTime { get; set; } = DateTime.Now; //最后更新时间
        public DateTime CreateTime { get; set; } = DateTime.Now; //创建时间

        public bool IsDeleted { get; set; } = false; // 默认未删除

        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        // 是否可借（未下架 + 有库存）
        [NotMapped]
        public bool IsAvailable => Status == 1 && BorrowedCopies < TotalCopies;
    }
}