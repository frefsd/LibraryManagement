using LibraryManagement.LM.Common.AppDbContext;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.LM.Service.Services.Impl
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入
        public AdminService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// 创建管理员
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Admin?> ValidateCredentialsAsync(string username, string password)
        {
            var admin = await _applicationDbContext.Admin
                .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);
            if (admin == null) return null;

            if (BCrypt.Net.BCrypt.Verify(password, admin.Password))
            {
                admin.LastLoginAt = DateTime.Now;
                await _applicationDbContext.SaveChangesAsync();
                return admin;
            }
            return null;
        }
    }
}
