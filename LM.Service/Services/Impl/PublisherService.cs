using LibraryManagement.LM.Common.AppDbContext;
using LibraryManagement.LM.Pojo.Models;
using LibraryManagement.LM.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.LM.Service.Services.Impl
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
