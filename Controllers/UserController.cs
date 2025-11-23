using LibraryManagement.DTO;
using LibraryManagement.Models;
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
        private readonly IBorrowService _borrowService;

        public UserController(IUserService userService, IBorrowService borrowService)
        {
            _userService = userService;
            _borrowService = borrowService;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryPage(
            [FromQuery] string? name,
            [FromQuery] string? phone,
            [FromQuery] string? cardNumber,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(name, phone, cardNumber, page, pageSize);
            return Ok(new { code = true, msg = "查询成功", data = result });
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> User(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { code = false, msg = "用户不存在" });
            }
            return Ok(new { code = true, msg = "查询成功", data = user });
        }

        /// <summary>
        /// 新增用户信息
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { code = false, msg = "参数验证失败", errors });
            }

            var user = new User
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                CardNumber = dto.CardNumber,
                CreateTime = DateTime.Now,
                Status = 1
            };

            var created = await _userService.AddUserAsync(user);
            return Ok(new { code = true, msg = "用户创建成功", data = created });
        }

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { code = false, msg = "参数验证失败", errors });
            }

            var existing = await _userService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { code = false, msg = "用户不存在" });
            }

            var user = new User
            {
                Id = id,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                CardNumber = dto.CardNumber,
                Status = existing.Status,
                CreateTime = existing.CreateTime
            };

            var updated = await _userService.UpdateUserAsync(user);
            return Ok(new { code = true, msg = "更新成功", data = updated });
        }

        /// <summary>
        /// 删除用户信息
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 0) return NotFound("无效的用户ID");
            //判断是否存在未归还的借阅记录
            var hasActiveBorrow = await _borrowService.HasActiveBorrowAsync(id);
            if (hasActiveBorrow)
            {
                //返回业务错误
                return BadRequest(new { code = false, msg = "该用户有未归还的书籍，不能删除"});
            }

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new { code = false, msg = "用户不存在或已被删除"});
            }

            return Ok(new { code = true, msg = "删除成功" });
        }

        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Status(int id, [FromBody] ChangeStatusDto dto)
        {
            var updated = await _userService.ChangeStatusAsync(id, dto.Status);
            if (!updated)
            {
                return NotFound(new { code = false, msg = "用户不存在" });
            }
            string msg = dto.Status ? "用户已启用" : "用户已禁用";
            return Ok(new { code = true, msg = msg });
        }
    }
}