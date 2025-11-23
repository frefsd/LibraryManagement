using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DTO
{
    public class ChangeStatusDto
    {
        [Required]
        public int Status { get; set; }
    }
}
