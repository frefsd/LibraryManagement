using LibraryManagement.Repository;
using LibraryManagement.Repository.Impl;

namespace LibraryManagement.Services.Impl
{
    public class BorrowService:IBorrowService
    {
        private readonly IBorrowRepository _borrowRepository;

        //依赖注入
        public BorrowService(IBorrowRepository borrowRepository)
        {
            _borrowRepository = borrowRepository;
        }
    }
}
