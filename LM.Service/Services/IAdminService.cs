using LibraryManagement.LM.Pojo.Models;

namespace LibraryManagement.LM.Service.Services
{
    /// <summary>
    /// 管理员接口实现
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
