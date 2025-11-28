using LibraryManagement.Models;
using LibraryManagement.Result;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 用户管理接口
    /// </summary>
    public interface IUserService
    {

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PageResult<User>> GetUsersAsync(string? name, string? phone, string? cardNumber, int page, int pageSize);

        /// <summary>
        /// 新增用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User?> AddUserAsync(User user);

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        Task<User?> UpdateUserAsync(User user);

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteUserAsync(int id);

       
        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// 判断用户的状态（1.正常 2.禁用）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<bool> ChangeStatusAsync(int id, int status);

        /// <summary>
        /// 检查用户是否有未归还的记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasActiveBorrowAsync(int userId);

    }
}
