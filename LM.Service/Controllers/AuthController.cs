using LibraryManagement.LM.Pojo.DTO;
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
    /// 登录
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IConfiguration _configuration;

        //依赖注入
        public AuthController(IAdminService adminService, IConfiguration configuration)
        {
            _adminService = adminService;
            _configuration = configuration;
        }

        /// <summary>
        /// 注册系统管理员
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            //获取用户名和密码
            var admin = await _adminService.ValidateCredentialsAsync(login.Username, login.Password);
            //判断
            if (admin == null)
                return Ok(new { code = false, msg = "用户民或者密码错误" });

            //生成JWT令牌
            var token = GenerateJwtToken(admin);

            return Ok(new
            {
                code = true,
                msg = "登录成功",
                data = new
                {
                    token,
                    user = new { admin.Id, admin.Username, admin.RealName, admin.Role }
                }
            });
        }

        private string GenerateJwtToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new  Claim(JwtRegisteredClaimNames.Name, admin.Username),
                new Claim("role", admin.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), //登录过期时间为24小时
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
