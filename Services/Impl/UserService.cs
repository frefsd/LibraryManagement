using LibraryManagement.Repository;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 用户管理模块
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        //依赖注入
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
    }
}
