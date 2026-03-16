using LibraryManagement.LM.Pojo.dto;

namespace LibraryManagement.LM.Pojo.vo
{
    public class BorrowStatsVO
    {
        public List<BorrowStatItemDTO> Stats { get; set; } = new();
        public BorrowSummaryDTO Summary { get; set; } = new();
    }
}
