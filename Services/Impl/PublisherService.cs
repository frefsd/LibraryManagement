using LibraryManagement.AppDbContext;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services.Impl
{
    /// <summary>
    /// 出版社
    /// </summary>
    public class PublisherService:IPublisherService
    {
        public readonly ApplicationDbContext _applicationDbContext;
        //依赖注入
        public PublisherService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        //获取所有出版社
        public async Task<List<Publisher>> AllAsync() => await _applicationDbContext.Publishers.ToListAsync();
    }
}
