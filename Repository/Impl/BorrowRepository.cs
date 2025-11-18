using LibraryManagement.AppDbContext;

namespace LibraryManagement.Repository.Impl
{

    public class BorrowRepository:IBorrowRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        //依赖注入 
        public BorrowRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
    }
}
