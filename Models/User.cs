using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 用户表（读者/借阅者）
    /// </summary>
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; } // 主键，自增ID

        public string? Name { get; set; } // 用户真实姓名
        public string? Phone { get; set; } // 手机号码
        public string? Email { get; set; } // 电子邮箱
        public string? CardNumber { get; set; } // 借书卡号（唯一标识读者）
        public int Status { get; set; } = 1; // 用户状态：1-正常，0-禁用
        public DateTime CreateTime { get; set; } // 账户创建时间
    }
}