using LibraryManagement.LM.Pojo.dto;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.LM.Service.Controllers
{
    /// <summary>
    /// 认证授权接口（登录、JWT颁发）
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IConfiguration _configuration;

        public AuthController(IAdminService adminService, IConfiguration configuration)
        {
            _adminService = adminService;
            _configuration = configuration;
        }

        /// <summary>
        /// 管理员登录
        /// </summary>
        /// <param name="login">登录参数：用户名、密码</param>
        /// <returns>返回JWT令牌与用户信息</returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest(new { code = false, msg = "用户名和密码不能为空" });
            }

            var admin = await _adminService.ValidateCredentialsAsync(login.Username.Trim(), login.Password);

            if (admin == null)
                return Unauthorized(new { code = false, msg = "用户名或密码错误" });

            var token = GenerateJwtToken(admin);

            // 将 token 同时写入 HttpOnly cookie，以提高安全性（前端无法通过 JS 读取）
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                // 使用 None 以允许跨站点（不同端口）时浏览器发送 Cookie（需配合 AllowCredentials）
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(24)
            };
            Response.Cookies.Append("access_token", token, cookieOptions);

            return Ok(new
            {
                code = true,
                msg = "登录成功",
                data = new
                {
                    user = new { admin.Id, admin.Username, admin.RealName, admin.Role }
                }
            });
        }

        /// <summary>
        /// 生成JWT令牌
        /// </summary>
        /// <param name="admin">管理员实体</param>
        /// <returns>JWT字符串</returns>
        private string GenerateJwtToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, admin.Username),
                new Claim("role", admin.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}