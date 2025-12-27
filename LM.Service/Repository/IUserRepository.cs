using LibraryManagement.LM.Pojo.Models;

namespace LibraryManagement.LM.Service.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardName"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        Task<List<User>> GetPagedAsync(string? name, string? phone, string? cardNumber, int page, int pageSize);

        /// <summary>
        /// 新增用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User?> AddUserAsync(User user);

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// 根据id删除用户信息
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
        /// 通过用户姓名，手机号，或者借书卡号查询用户信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        Task<int> GetCountAsync(string? name, string? phone, string? cardNumber);

        /// <summary>
        /// 判断借书卡号是否存在
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        Task<bool> ExistsByCardNumberAsync(string cardNumber);

        /// <summary>
        /// 获取可用的用户（未禁用）
        /// </summary>
        /// <returns></returns>
        IQueryable<User> GetQueryableAsync();

        /// <summary>
        /// 设置用户的状态（1.启用 2.禁用）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<bool> SetStatusAsync(int id, int status);

        /// <summary>
        /// 检查用户是否有未归还的借阅记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasActiveBorrowAsync(int userId);

        /// <summary>
        /// 通过用户名查询该用户是否存在
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<User?> GetByUsernameAsync(string username);
    }
}
