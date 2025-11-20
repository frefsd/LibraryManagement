using LibraryManagement.AppDbContext;
using LibraryManagement.Models;

namespace LibraryManagement.Repository.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入
        public UserRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// 通过id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _applicationDbContext.Users.FindAsync(id);
        }

        public IQueryable<User> GetQueryableAsync()
        {
            return _applicationDbContext.Users;
        }
    }
}
