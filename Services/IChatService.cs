
namespace LibraryManagement.Services
{
    public interface IChatService
    {
        /// <summary>
        /// 采用流式方式回答用户的信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> AskStreamAsync(string message);
    }
}
