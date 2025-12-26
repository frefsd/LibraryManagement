using Azure.Core.Serialization;
using LibraryManagement.Exceptions;
using LibraryManagement.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LibraryManagement.Services.Impl
{
    public class QwenApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        //依赖注入
        public QwenApiService(HttpClient httpClient, IOptions<DashScopeOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "qwen-turbo",
                input = new
                {
                    messages = new[]
                    {
                        new {role = "system", content = "你是一个专业、简洁的图书管理员助手."},
                        new {role = "user", content = prompt}
                    }
                },
                parameters = new
                {
                    result_format = "message"
                }
            };
            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(
                "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new DomainException($"Qwen API 调用失败：{response.StatusCode},{error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("output")
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? "抱歉，我暂时无法回答这个问题。";
        }
    }
}
