using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DTO
{
    public class ChangeStatusDto
    {
        [Required]
        public bool Status { get; set; }
    }
}
