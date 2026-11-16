using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class AiChatHelper
    {
        private static readonly HttpClient _httpClient;
        private const string APIKEY = "JD_API_KEY"; // 环境变量名称
        private const string API_BASE_URL = "https://api.deepseek.com/chat/completions"; // 环境变量名称
        static AiChatHelper()
        {
            // .NET Framework 4.8 访问现代 HTTPS API 必须显式开启 TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            _httpClient = new HttpClient
            {
                // 流式请求需要无限期超时，防止大模型长文本输出时被中断
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        /// <summary>
        /// 封装 GPT / DeepSeek REST API 的流式（Stream）请求
        /// </summary>
        /// <param name="apiUrl">API 地址，例如：https://api.deepseek.com/chat/completions</param>
        /// <param name="apiKey">API Key</param>
        /// <param name="model">模型名称，例如：deepseek-chat 或 gpt-4o</param>
        /// <param name="messages">历史消息列表，格式为 [{"role": "user", "content": "你好"}]</param>
        /// <param name="onTokenReceived">收到每个 Token 片段时的回调（用于打字机效果）</param>
        /// <param name="cancellationToken">取消令牌（用于停止生成）</param>
        public static async Task SendStreamChatAsync(
            string apiUrl,
            string apiKey,
            string model,
            List<Dictionary<string, string>> messages,
            Action<string> onTokenReceived,
            CancellationToken cancellationToken = default)
        {
            // 构造符合 OpenAI 标准的 Payload
            var payload = new
            {
                model = model,
                stream = true,
                messages = messages
            };

            string jsonBody = JsonConvert.SerializeObject(payload);

            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // 核心：ResponseHeadersRead 确保收到响应头后立即开始流式读取，不缓存整个 Body
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"API 请求失败 ({(int)response.StatusCode}): {errorDetail}");
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            string line = await reader.ReadLineAsync();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // 解析 SSE 协议的 data 行
                            if (line.StartsWith("data:"))
                            {
                                string data = line.Substring(5).Trim();
                                if (data == "[DONE]") break;

                                try
                                {
                                    var jsonObj = JObject.Parse(data);
                                    var deltaContent = jsonObj["choices"]?[0]?["delta"]?["content"]?.ToString();

                                    if (!string.IsNullOrEmpty(deltaContent))
                                    {
                                        onTokenReceived?.Invoke(deltaContent);
                                    }
                                }
                                catch (JsonException)
                                {
                                    // 忽略网络心跳包或残缺 JSON 片段
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 支持图文多模态的流式聊天请求
        /// </summary>
        public static async Task SendStreamChatAttachAsync(
            string apiUrl,
            string apiKey,
            string model,
            List<Dictionary<string, object>> messages, // 升级为 object 支持复杂多模态结构
            Action<string> onTokenReceived,
            CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = model,
                stream = true,
                messages = messages,
                max_tokens = 4096
            };

            string jsonBody = JsonConvert.SerializeObject(payload);

            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"API 请求失败 ({(int)response.StatusCode}): {errorDetail}");
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            string line = await reader.ReadLineAsync();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            if (line.StartsWith("data:"))
                            {
                                string data = line.Substring(5).Trim();
                                if (data == "[DONE]") break;

                                try
                                {
                                    var jsonObj = JObject.Parse(data);
                                    var deltaContent = jsonObj["choices"]?[0]?["delta"]?["content"]?.ToString();

                                    if (!string.IsNullOrEmpty(deltaContent))
                                    {
                                        onTokenReceived?.Invoke(deltaContent);
                                    }
                                }
                                catch (JsonException) { }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 封装普通 POST 请求（非流式一次性返回）
        /// </summary>
        public static async Task<string> SendPostAsync(string apiUrl, string apiKey, object payload)
        {
            string jsonBody = JsonConvert.SerializeObject(payload);
            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var response = await _httpClient.SendAsync(request))
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"POST 请求失败: {responseString}");
                    }
                    return responseString;
                }
            }
        }

        /// <summary>
        /// 测试用例
        /// </summary>
        /// <returns></returns>
        public static async Task Simple()
        {
           var userPrompt = "你好，请用中文回答我。";
            // 1. 初始化对话历史上下文
            List<Dictionary<string, string>> _apiHistory = new List<Dictionary<string, string>>();
            _apiHistory.Add(new Dictionary<string, string> { { "role", "user" }, { "content", userPrompt } });

            string apiUrl = "https://api.deepseek.com/chat/completions"; // 或其他 OpenAI 兼容地址
            string apiKey = "你的-API-Key";
            string model = "deepseek-chat";

            StringBuilder fullReply = new StringBuilder();

            try
            {
                // 调用封装好的静态工具类
                await AiChatHelper.SendStreamChatAsync(
                    apiUrl,
                    apiKey,
                    model,
                    _apiHistory,
                    token =>
                    {
                        // 回调中拿到流式返回的片段
                        fullReply.Append(token);
                        // 此处可结合前面提到的节流队列更新 UI
                    },
                    cancellationToken: default);

                // 3. AI 回复完成后，将完整回复追加到历史上下文（用于多轮对话）
                _apiHistory.Add(new Dictionary<string, string> { { "role", "assistant" }, { "content", fullReply.ToString() } });
            }
            catch (Exception ex)
            {
                // 异常处理
                System.Windows.MessageBox.Show($"错误: {ex.Message}");
                await SimpleMultiImageAsync("", null);
            }
        }

        /// <summary>
        /// 发送带图片和文字的消息
        /// </summary>
        private static async Task SimpleMultiImageAsync(string textPrompt, List<string> localImagePaths)
        {
            // 1. 构造 OpenAI 标准的多媒体 content 数组
            var contentList = new List<object>();
            // 维护的历史上下文（支持多模态 object）
            List<Dictionary<string, object>> _apiHistory = new List<Dictionary<string, object>>();
            // 如果有文字，加入文字部分
            if (!string.IsNullOrEmpty(textPrompt))
            {
                contentList.Add(new { type = "text", text = textPrompt });
            }

            foreach (var png in localImagePaths)
            {
                // 如果有图片，将图片转为 Base64 并加入部分
                if (!string.IsNullOrEmpty(png) && File.Exists(png))
                {
                    string base64ImageUrl = ImageHelper.ImageFileToBase64(png);
                    contentList.Add(new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = base64ImageUrl,
                            detail = "auto" // 可选: low, high, auto
                        }
                    });
                }
            }

            // 2. 将当前用户消息加入上下文
            var userMessage = new Dictionary<string, object>
            {
                { "role", "user" },
                { "content", contentList } // 传入包含文本和图片的数组
            };
            _apiHistory.Add(userMessage);

            string apiUrl = API_BASE_URL; // 或兼容多模态的模型地址（如 DeepSeek-V2.5, GPT-4o 等）
            string apiKey = Environment.GetEnvironmentVariable(APIKEY); ;
            string model = "gpt-4o"; // 确保选择支持 Vision 的大模型

            // 3. 调用流式传输（复用前面的节流缓冲队列和 UI 刷新逻辑）
            await AiChatHelper.SendStreamChatAttachAsync(
                apiUrl,
                apiKey,
                model,
                _apiHistory,
                token => {
                    // 流式接收回调处理...
                });
        }
    }
}
