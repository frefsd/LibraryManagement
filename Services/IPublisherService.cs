using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    /// <summary>
    /// 出版社管理接口
    /// </summary>
    public interface IPublisherService
    {
        //获取所有出版社
        Task<List<Publisher>> AllAsync();
    }
}
