using LibraryManagement.Models;

namespace LibraryManagement.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// 通过id获取用户信息
        /// </summary>
        /// <returns></returns>
        Task<User?> GetByIdAsync(int id);
    }
}
