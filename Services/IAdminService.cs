using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// 创建管理员
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Admin?> ValidateCredentialsAsync(string username, string password);
        //Task CreateDefaultAdminIfNotExistsAsync();
    }
}
