using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 借阅记录表
    /// </summary>
    [Table("BorrowRecord")]
    public class BorrowRecord
    {
        [Key]
        public int Id { get; set; } // 主键，自增ID

        public int BookId { get; set; } // 关联图书的 Id
        public int UserId { get; set; } // 关联用户的 Id

        // 导航属性：借阅的图书详情
        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }

        // 导航属性：借阅用户信息
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime BorrowDate { get; set; } // 借阅日期
        public DateTime ReturnDate { get; set; } // 应还日期（非实际归还时间）
        public int Status { get; set; } = 1; // 借阅状态：1-借阅中，2-已归还，3-逾期
        public DateTime CreateTime { get; set; } // 记录创建时间
        public DateTime UpdateTime { get; set; } // 记录最后更新时间（如归还时更新）
    }
}