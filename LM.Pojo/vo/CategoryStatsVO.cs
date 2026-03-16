using LibraryManagement.LM.Pojo.dto;

namespace LibraryManagement.LM.Pojo.vo
{
    public class CategoryStatsVO
    {
        public List<CategoryStatsItemDTO> Stats { get; set; } = new();
        public CategorySummaryDTO Summary { get; set; } = new();
    }
}
