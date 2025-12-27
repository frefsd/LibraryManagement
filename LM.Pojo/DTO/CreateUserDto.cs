using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.LM.Pojo.DTO
{
    /// <summary>
    /// 新增用户请求参数
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "姓名不能为空")]
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        [Required(ErrorMessage = "借书卡号不能为空")]
        public string? CardNumber { get; set; }
    }
}
