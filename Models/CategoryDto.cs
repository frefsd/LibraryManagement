namespace LibraryManagement.Models
{
    public class CategoryDto
    {
        public string? Name { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; } = 1; //1.启用 0.禁止

    }
}
