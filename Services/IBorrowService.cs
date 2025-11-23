using LibraryManagement.DTO;
using LibraryManagement.Result;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 借阅管理模块接口
    /// </summary>
    public interface IBorrowService
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PageResult<BorrowRecordDto>> GetPageAsync(int page, int pageSize);
        /// <summary>
        /// 获取借阅信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task BorrowAsync(BorrowRequestDto dto);
        /// <summary>
        /// 获取借阅人是否归还图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task ReturnAsync(int id);

        /// <summary>
        /// 检查图书是否正在被借阅（存在未归还记录）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasActiveBorrowAsync(int userId);
    }
}
