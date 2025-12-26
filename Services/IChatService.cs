namespace LibraryManagement.Services
{
    public interface IChatService
    {
        Task<string> AskAsync(string message);
    }
}
