using LibraryManagement.LM.Pojo.Models;

namespace LibraryManagement.LM.Service.Services
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
