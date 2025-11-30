using LibraryManagement.DTO;
using LibraryManagement.Result;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 借阅管理接口实现
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
        /// 续借图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task RenewAsync(int id);

        /// <summary>
        /// 检查指定用户是否存在未归还的借阅记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasActiveBorrowAsync(int userId);

        /// <summary>
        /// 检查指定图书是否存在未归还的借阅记录
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        Task<bool> HasUnreturnRecordAsync(int bookId);
    }
}
