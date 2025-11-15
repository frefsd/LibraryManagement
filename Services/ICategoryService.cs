using LibraryManagement.Models;

namespace LibraryManagement.Services
{
    public interface ICategoryService
    {
        //获取所有分类
        Task<List<Category>> AddAsync();
    }
}
