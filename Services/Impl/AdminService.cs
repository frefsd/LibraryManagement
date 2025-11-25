using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
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


        /*public async Task CreateDefaultAdminIfNotExistsAsync()
        {
            //判读是否数据库中是否有管理员
            if (await _applicationDbContext.Admin.AnyAsync())
                return;

            var defaultAdmin = new Admin
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                RealName = "系统管理员",
                Role = "SuperAdmin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _applicationDbContext.Admin.Add(defaultAdmin);
            await _applicationDbContext.SaveChangesAsync();

            Console.WriteLine("默认管理员已创建");
        }*/

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
