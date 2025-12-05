using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DTO
{
    /// <summary>
    /// 启用/禁用用户状态
    /// </summary>
    public class ChangeStatusDto
    {
        [Required]
        public int Status { get; set; } //1.正常 0.禁用
    }
}
