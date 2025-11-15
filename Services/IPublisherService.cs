using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    public interface IPublisherService
    {
        //获取所有出版社
        Task<List<Publisher>> AllAsync();
    }
}
