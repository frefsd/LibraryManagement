using LibraryManagement.Models;
using LibraryManagement.Repository;
using LibraryManagement.Result;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

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

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PageResult<User>> GetUsersAsync(string? name, string? phone, string? cardNumber, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var skip = (page - 1) * pageSize;

            var users = await _userRepository.GetPagedAsync(name, phone, cardNumber, page, pageSize);
            var total = await _userRepository.GetCountAsync(name, phone, cardNumber);

            return new PageResult<User>
            {
               Total = total,
               Page = page,
               PageSize = pageSize,
               Rows = users
            };
        }
        /// <summary>
        /// 新增用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<User?> AddUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.CardNumber)) throw new ArgumentException("借书卡号不能为空");
            if (await _userRepository.ExistsByCardNumberAsync(user.CardNumber)) throw new InvalidOperationException($"借书卡号{user.CardNumber}已存在");

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
        /// <exception cref="NotImplementedException"></exception>
        public async Task<User?> UpdateUserAsync(User user)
        {
            //获取用户
            var existing = await _userRepository.GetByIdAsync(user.Id);
            if (existing == null) throw new KeyNotFoundException("用户不存在");

                if (!string.Equals(existing.CardNumber, user.CardNumber))
            {

                if (await _userRepository.ExistsByCardNumberAsync(user.CardNumber))
                    throw new InvalidOperationException($"借书卡号{user.CardNumber}已存在");
            }

            existing.Name = user.Name;
            existing.Phone = user.Phone;
            existing.Email = user.Email;
            existing.CardNumber = user.CardNumber;

            await _userRepository.UpdateAsync(existing);
            return existing;
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> ChangeStatusAsync(int id, bool status)
        {
          return await _userRepository.SetStatusAsync(id, status ? 1 : 0);
        }

    }
}
