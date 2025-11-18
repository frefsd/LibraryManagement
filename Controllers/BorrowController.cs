using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// 借阅管理模块
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class BorrowController:ControllerBase
    {
        private readonly IBorrowService _borrowService;

        //依赖注入
        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }
    }
}
