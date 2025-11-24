using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DTO
{
    public class ChangeStatusDto
    {
        [Required]
        public int Status { get; set; } //1.正常 0.禁用
    }
}
