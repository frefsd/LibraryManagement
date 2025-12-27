using LibraryManagement.LM.Common.Exceptions;
using LibraryManagement.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LibraryManagement.LM.Service.Services.Impl
{
    public class QwenApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

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
                stream = true,
                stream_options = new { include_usage = true }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post,
                "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new DomainException($"Qwen API 调用失败：{response.StatusCode},{error}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var buffer = new StringBuilder();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var jsonData = line["data:".Length..].Trim();

                    if (jsonData == "[DONE]")
                    {
                        if (buffer.Length > 0)
                        {
                            yield return buffer.ToString();
                            buffer.Clear();
                        }
                        yield break;
                    }

                    string? text = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(jsonData);

                        if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                            choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];

                            if (choice.TryGetProperty("delta", out var delta) &&
                                delta.TryGetProperty("content", out var contentProp))
                            {
                                text = contentProp.GetString();

                                if (!string.IsNullOrEmpty(text))
                                {
                                    text = RemoveHtmlTags(text);
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        buffer.Append(text);
                        if (buffer.Length >= 10)
                        {
                            yield return buffer.ToString();
                            buffer.Clear();
                        }
                    }
                }
            }

            if (buffer.Length > 0)
            {
                yield return buffer.ToString();
            }
        }

        private static string RemoveHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var regex = new Regex(@"<[^>]+>", RegexOptions.IgnoreCase);
            return regex.Replace(html, "");
        }
    }
}