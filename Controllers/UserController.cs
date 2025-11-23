using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 用户管理模块
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        //依赖注入
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
    }
}
