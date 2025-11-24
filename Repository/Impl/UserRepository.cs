using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cardName"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public async Task<List<User>> GetPagedAsync(string? name, string? phone, string? cardNumber, int page, int pageSize)
        {
            var query = _applicationDbContext.Users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.Name!.Contains(name));
            if (!string.IsNullOrEmpty(phone))
                query = query.Where(u => u.Phone!.Contains(phone));
            if (!string.IsNullOrEmpty(cardNumber))
                query = query.Where(u => u.CardNumber!.Contains(cardNumber));

            return await query
                .OrderBy(u => u.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 新增用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public async Task<User?> AddUserAsync(User user)
        {
            await _applicationDbContext.Users.AddAsync(user);
            await _applicationDbContext.SaveChangesAsync(); //保存
            return user;
        }

        /// <summary>
        /// 根据id删除用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _applicationDbContext.Users.FindAsync(id);
            if (user == null) return false;

            _applicationDbContext.Users.Remove(user);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 通过借书卡号判读该用户是否存在
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> ExistsByCardNumberAsync(string cardNumber) =>
            await _applicationDbContext.Users.AnyAsync(u => u.CardNumber == cardNumber);
       

        /// <summary>
        /// 通过id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _applicationDbContext.Users.FindAsync(id);
        }

        public async Task<int> GetCountAsync(string? name, string? phone, string? cardNumber)
        {
            var query = _applicationDbContext.Users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.Name!.Contains(name));
            if (!string.IsNullOrEmpty(phone))
                query = query.Where(u => u.Phone!.Contains(phone));
            if (!string.IsNullOrEmpty(cardNumber))
                query = query.Where(u => u.CardNumber!.Contains(cardNumber));

            return await query.CountAsync();
        }

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task UpdateAsync(User user)
        {
            _applicationDbContext.Entry(user).State = EntityState.Modified;
            await _applicationDbContext.SaveChangesAsync();
        }

      
        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SetStatusAsync(int id, int status)
        {
            var user = await _applicationDbContext.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = status;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 获取未被禁用的用户（未禁用）
        /// </summary>
        /// <returns></returns>
        public IQueryable<User> GetQueryableAsync()
        {
            return _applicationDbContext.Users;
        }

        /// <summary>
        /// 检查用户是否有未归还的借阅记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> HasActiveBorrowAsync(int userId)
        {
            return await _applicationDbContext.BorrowRecords.AnyAsync(br => br.UserId == userId && br.ActualReturnDate == null);
        }
    }
}
