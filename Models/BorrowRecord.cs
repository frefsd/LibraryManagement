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

        public DateTime CreateTime { get; set; } // 记录创建时间
        public DateTime UpdateTime { get; set; } // 记录最后更新时间（如归还时更新）

        public DateTime BorrowDate { get; set; } // 借出时间
        public DateTime DueDate { get; set; } // 应还时间（如 BorrowDate + 30天）
        public DateTime? ActualReturnDate { get; set; } // 实际归还时间（null=未还）
        public int Status { get; set; } = 1;  // 状态：1-借阅中，2-已归还，3-逾期
        public int? RenewCount { get; set; } // 续借 0-未续借 1-续借

        // 计算是否逾期
        [NotMapped]
        public bool IsOverdue => ActualReturnDate == null && DateTime.Now > DueDate;
    }
}