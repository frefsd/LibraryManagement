
namespace LibraryManagement.Services.Impl
{
    public class ChatService : IChatService
    {
        private readonly QwenApiService _qwenApiService;

        //依赖注入
        public ChatService(QwenApiService qwenApiService)
        {
            _qwenApiService = qwenApiService;
        }

        /// <summary>
        /// 采用流式方式回答用户的信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
       public async IAsyncEnumerable<string> AskStreamAsync(string message)
        {
            var prompt = $@"你是一个智能图书管理员助手，可以回答关于图书、作者、文学、阅读建议、出版信息等问题。
                            如果问题与图书无关，也可以基于你的通用知识进行友好、简洁的回答。
                            用户提问：{message}
                            请直接给出自然、流畅的回答。";

            await foreach (var token in _qwenApiService.GenerateResponseStreamAsync(prompt))
            {
                yield return token;
            }
        }
    }
}
