using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    [Table("Admin")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 登录用户名（唯一）
        /// </summary>
        [Required, MaxLength(50)]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度为3-50个字符")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希值（BCrypt 加密存储）
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string? RealName { get; set; }

        /// <summary>
        /// 角色（预留，如：SuperAdmin / Librarian）
        /// </summary>
        public string Role { get; set; } = "Admin";

        /// <summary>
        /// 账户是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
