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

        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt)
        {
            var requestBody = new
            {
                model = "qwen-turbo",
                messages = new[]
                    {
                    new { role = "system", content = "你是一个专业、简洁的图书管理员助手." },
                    new { role = "user", content = prompt }
                },
                stream = true

            };
            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(
                "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new DomainException($"Qwen API 调用失败：{response.StatusCode},{error}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("data:"))
                {
                    var jsonData = line["data:".Length..].Trim();
                    if (jsonData == "[DONE]") break;

                    string? text = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(jsonData);
                        text = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("delta")
                              .GetProperty("content")
                              .GetString();
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        yield return text;
                    }
                }
               
            }
        }
    }
}
