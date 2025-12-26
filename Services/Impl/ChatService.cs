
namespace LibraryManagement.Services.Impl
{
    public class ChatService : IChatService
    {
        private readonly IBookService _bookService;
        private readonly QwenApiService _qwenApiService;

        //依赖注入
        public ChatService(IBookService bookService, QwenApiService qwenApiService)
        {
            _bookService = bookService;
            _qwenApiService = qwenApiService;
        }

        public async Task<string> AskAsync(string message)
        {
            //从消息中提取可能的书名
            var bookTitle = ExtractBookTtile(message) ?? message;
            var books = await _bookService.SearchBookByKeywordAsync(bookTitle);

            var context = books.Any()
                ? string.Join("\n", books.Select(b => $"书名：《{b.Name}, 作者：{b.Author}, 分类：{b.Category?.Name ?? "未知"}")) : "暂无相关图书信息。";

            var prompt = $@"你是一个图书管理员助手。
                            以下是系统中的图书信息：{context}
                            用户提问：{message}
                            请基于以上信息回答问题。如果不知道，请说“暂未找到相关信息”。
                            不要编造内容，不要提及“根据提供的信息”等机械语句。";

            return await _qwenApiService.GenerateResponseAsync(prompt);
           
        }

        private string? ExtractBookTtile(string message)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, @"《(.+?)》");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
