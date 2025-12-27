using LibraryManagement.LM.Common.Exceptions;
using LibraryManagement.LM.Common.Result;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Repository;
using LibraryManagement.LM.Service.Services;

namespace LibraryManagement.LM.Service.Services.Impl
{
    /// <summary>
    /// 用户管理业务代码开发
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        //依赖注入
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardNumber"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageResult<User>> GetUsersAsync(string? name, string? phone, string? cardNumber, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var skip = (page - 1) * pageSize;

            //获取用户列表
            var users = await _userRepository.GetPagedAsync(name, phone, cardNumber, page, pageSize);
            //根据用户名，手机号或者借书卡号获取用户信息
            var total = await _userRepository.GetCountAsync(name, phone, cardNumber);

            return new PageResult<User>
            {
                Total = total, //总用户量
                Page = page, //当前页面
                PageSize = pageSize, //每页展示的数据
                Rows = users //总用户列表
            };
        }

        /// <summary>
        /// 新增用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="DomainException"></exception>
        public async Task<User?> AddUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.CardNumber))
                throw new DomainException("借书卡号不能为空");
            if (await _userRepository.ExistsByCardNumberAsync(user.CardNumber))
                throw new DomainException($"借书卡号{user.CardNumber}已存在");
            user.CreateTime = DateTime.Now;
            user.Status = 1; //默认启用

            return await _userRepository.AddUserAsync(user);
        }

        /// <summary>
        /// 根据id删除用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserAsync(int id) => await _userRepository.DeleteUserAsync(id);

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="DomainException"></exception>
        public async Task<User?> UpdateUserAsync(User user)
        {
            //获取用户
            var existing = await _userRepository.GetByIdAsync(user.Id);
            if (existing == null)
                throw new DomainException("用户不存在");

            //判断用户的借书卡号existing.CardNumber == user.CardNumber
            if (!string.Equals(existing.CardNumber, user.CardNumber))
            {

                if (await _userRepository.ExistsByCardNumberAsync(user.CardNumber))
                    throw new DomainException($"借书卡号{user.CardNumber}已存在");
            }

            existing.Phone = user.Phone;
            existing.Email = user.Email;
            existing.CardNumber = user.CardNumber;
            existing.Status = user.Status;

            await _userRepository.UpdateAsync(existing);
            return existing;
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<bool> ChangeStatusAsync(int id, int status)
        {
            return await _userRepository.SetStatusAsync(id, status);
        }

        /// <summary>
        /// 检查用户是否有未归还的借阅记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> HasActiveBorrowAsync(int userId)
        {
            return await _userRepository.HasActiveBorrowAsync(userId);
        }
    }
}
