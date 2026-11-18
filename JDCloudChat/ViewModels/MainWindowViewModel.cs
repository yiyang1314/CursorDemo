using AyuUI.Controll.Commands;
using AyuUI.Controll.Controls;
using AyuUI.Controll.Dialogs;
using AyuUI.Controll.Helpers;
using AyuUI.Controll.IOC.Attributes;
using AyuUI.Controll.IOC.Simple;
using AyuUI.Controll.Messaging;
using AyuUI.Controll.Toasts;
using AyuUI.Controll.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace JDCloudChat.ViewModels
{
    [Service(ServiceLifetime.Singleton)]
    public class MainWindowViewModel: ViewModelBase
    {
        private ChatMessageUI mMessage =new ChatMessageUI();

        public ChatMessageUI UIMessage
        {
            get { return mMessage ; }
            set { SetProperty(ref mMessage, value); }
        }

        private ChatMessageUI mreqMessage = new ChatMessageUI();

        public ChatMessageUI ReqMessage
        {
            get { return mreqMessage; }
            set { SetProperty(ref mreqMessage, value); }
        }


        private readonly AyuMessenger messenger;
        public ObservableCollection<NavigationItem> MenuItems { get; set; }
        public ObservableCollection<ChatMessageUI> MsgItems { get; set; }
        public ObservableCollection<string> ModelItems { get; set; }

        private string selectedModel = string.Empty;

        public string SelectedModel
        {
            get { return selectedModel ; }
            set { SetProperty(ref selectedModel, value); }
        }


        private NavigationItem _selectedMenuItem;
        public NavigationItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set { SetProperty(ref _selectedMenuItem, value); }
        }
        public RelayCommand ToggleThemeCommand { get; set; }
        public RelayCommand<string> CopyCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public AsyncRelayCommand<string> SendMessageCommand { get; set; }
        private Visibility  sideHide = Visibility.Visible;

        public Visibility SideVisibied
        {
            get { return sideHide; }
            set { SetProperty(ref sideHide, value); }
        }

        private bool mIsEnableBtn = false;

        public bool IsEnableBtn
        {
            get { return mIsEnableBtn =false; }
            set { SetProperty(ref mIsEnableBtn, value); }
        }
        private CancellationTokenSource cancellationToken;
        public MainWindowViewModel(AyuMessenger messenger)
        {
            this.messenger = messenger;
            MenuItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Id = "home", Title = "首页概览", IconData = "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z", IsSelected = true },
                new NavigationItem { Id = "device", Title = "设备监控", IconData = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4Z" },
                new NavigationItem { Id = "recipe", Title = "配方管理", IconData = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z" },
                new NavigationItem { Id = "settings", Title = "系统设置", IconData = "M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.53,5.05 19.27,4.97 19.05,5.05L16.56,6.05C16.04,5.65 15.48,5.32 14.87,5.07L14.49,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.51,2.42L9.13,5.07C8.52,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.97 4.47,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.47,18.95 4.73,19.03 4.95,18.95L17.44,17.95C17.96,18.35 18.52,18.68 19.13,18.93L19.51,21.58C19.54,21.82 19.75,22 20,22H24C24.25,22 24.46,21.82 24.49,21.58L24.87,18.93C25.48,18.68 26.04,18.35 26.56,17.95L29.05,18.95C29.27,19.03 29.53,18.95 29.66,18.73L31.66,15.27C31.79,15.05 31.73,14.78 31.54,14.63L19.43,12.97Z" }
            };
            MsgItems=new ObservableCollection<ChatMessageUI>();
            ModelItems = new ObservableCollection<string>
            {
                "kimi-k3.1-pro",
                "kimi-k3",
                "GLM-5.3",
                "DeepSeek-V4-Pro-0813",
                "DeepSeek-V4.1-Flash-1017",
                "MiniMax-M3",
                "Kimi-K2.5",
                "JoyAI-VL-Interaction"
            };
            UIMessage = new ChatMessageUI { Role = "user", Content = "" };
            SelectedModel = ModelItems.FirstOrDefault();
            ToggleThemeCommand = new RelayCommand(Show);
            IsEnableBtn = true;
            SendMessageCommand = new AsyncRelayCommand<string>(SendMessageAsync);
            CopyCommand = new RelayCommand<string>((msg) => {
                if (!string.IsNullOrEmpty(msg))
                {
                    Clipboard.SetText(msg);
                }
            });
            StopCommand = new RelayCommand(() =>
            {
                IsEnableBtn = false;
                cancellationToken?.Cancel();
            });
        }

        private void Show()
        {
            SideVisibied = SideVisibied == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;  
            AyuToast.Info("侧边栏已切换");
        }

        private void SendMessage(string msg)
        {
            
        }
        private List<Dictionary<string, string>> _apiHistory = new List<Dictionary<string, string>>();
        // 在你的 ViewModel 或窗口类中：
        private readonly Queue<string> _tokenQueue = new Queue<string>();
        private DispatcherTimer _uiFlushTimer;

        private async Task SendMessageAsync(string msg)
        {
            if ((cancellationToken != null)|| string.IsNullOrEmpty(msg) || IsEnableBtn) return;
            ReqMessage = new ChatMessageUI { Role = "user", IsUser = true, Content = msg };
            // 1. 添加用户消息到 UI 和 API 历史
            MsgItems.Add(ReqMessage);
            // 2. 创建 AI 占位消息
            UIMessage = new ChatMessageUI { Role = "ai", Content = "思考中..." };
            MsgItems.Add(UIMessage);
            _apiHistory.Add(new Dictionary<string, string> {
                { "role", "user" },
                { "content", msg }
            });
            //if (!string.IsNullOrEmpty(msg))
            //{
            //    await Task.Delay(10000);
            //    IsEnableBtn = false;
            //    return;
            //}
            try
            {
                var sb = new StringBuilder();
                IsEnableBtn = true;
                cancellationToken = new CancellationTokenSource();

                // 3. 调用工具类，通过回调实时更新 UI
                object lockObj = new object();
                bool isUpdating = false;

                _uiFlushTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                _uiFlushTimer.Tick += (s, e) =>
                {
                    lock (_tokenQueue)
                    {
                        if (_tokenQueue.Count > 0)
                        {
                            while (_tokenQueue.Count > 0)
                            {
                                sb.Append(_tokenQueue.Dequeue());
                            }
                            // 统一赋值一次，减少触发绑定和 Markdown 解析的频率
                            UIMessage.Content += sb.ToString();
                        }
                    }
                };
                _uiFlushTimer.Start();

                // 3. 调用工具类，通过节流缓冲优化流式更新
                await AiChatHelper.SendStreamChatAsync(_apiHistory, (token) =>
                {
                    if (UIMessage.Content == "思考中...")
                    {
                        UIMessage.Content = string.Empty;
                        sb.Clear();
                    }

                    lock (_tokenQueue)
                    {
                        _tokenQueue.Enqueue(token);
                    }
                }, SelectedModel, cancellationToken);

                // 4. 将 AI 完整回复追加到历史上下文
                _apiHistory.Add(new Dictionary<string, string> {
                    { "role", "assistant" },
                    { "content", UIMessage.Content }
                });
            }
            catch (Exception ex)
            {
                UIMessage.Content = $"错误: {ex.Message}";
            }
            finally
            {
                IsEnableBtn = false;
                cancellationToken = null;
            }
        }
    }
    public static class AiChatHelper
    {
        // 1. 常量配置（模型与接口地址可配置，Key 从环境变量动态获取）
        private const string ApiUrl = "https://modelservice.jdcloud.com/v1/chat/completions";
        private const string ModelName = "DeepSeek-V4-Flash-0731";
        private static string APIKEY = "JD_API_KEY"; // 环境变量名称

        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// 发送流式聊天请求（基于 Newtonsoft.Json 实现）
        /// </summary>
        /// <param name="historyMessages">历史对话列表（包含 role 和 content）</param>
        /// <param name="onTokenReceived">每收到一个文本片段（Token）时的回调函数，用于实时刷新 UI</param>
        /// <returns>返回完整的 AI 回复内容</returns>
        public static async Task<string> SendStreamChatAsync(
            List<Dictionary<string, string>> historyMessages,
            Action<string> onTokenReceived,string ModelName = ModelName, CancellationTokenSource cancellationToken = null)
        {
            // 2. 从环境变量安全注入 API Key
            string apiKey = Environment.GetEnvironmentVariable(APIKEY);

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException($"未能在环境变量中找到 Key 为 '{APIKEY}' 的 API 密钥，请先在系统中配置环境变量。");
            }

            // 3. 使用 Newtonsoft.Json 序列化请求体载荷
            var requestPayload = new
            {
                model = ModelName,
                messages = historyMessages,
                stream = true
            };

            string jsonBody = JsonConvert.SerializeObject(requestPayload);

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // 4. 发送并流式读取
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                AyuToast.Error($"API 请求失败 (状态码: {response.StatusCode}): {errorContent}");
                LoggerHelper.Error($"API 请求失败 (状态码: {response.StatusCode}): {errorContent}");
                throw new HttpRequestException($"API 请求失败 (状态码: {response.StatusCode}): {errorContent}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            var fullReply = new StringBuilder();
            LoggerHelper.Info("AI 回复中.....");
            while (!reader.EndOfStream)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    AyuToast.Error($"停止任务请求");
                    break;
                }
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
                if (line == "data: [DONE]") break;

                string jsonStr = line.Substring(6); // 截掉 "data: " 前缀
                try
                {
                    var obj = JObject.Parse(jsonStr);

                    // 【修改点】先检查 choices 是否存在、是否为数组、且长度是否大于 0
                    var choicesToken = obj["choices"];
                    if (choicesToken is JArray choicesArray && choicesArray.Count > 0)
                    {
                        // 确保 delta 和 content 节点也安全存在
                        var firstChoice = choicesArray[0];
                        string token = firstChoice["delta"]?["content"]?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(token))
                        {
                            fullReply.Append(token);
                            // 触发回调，把当前片段传给 UI
                            onTokenReceived?.Invoke(token);
                        }
                    }
                }
                catch (JsonReaderException ex)
                {
                    // 忽略可能存在的半包 JSON 解析异常
                    LoggerHelper.Error($"JSON 解析跳过半包数据: {ex.Message}");
                }
            }
            AyuToast.Info("回答任务完成");
            return fullReply.ToString();
        }


        public static async Task Test() 
        {
            string modelname = "DeepSeek-V4-Flash-0731";
            List<Dictionary<string, string>> _apiHistory = new List<Dictionary<string, string>>();
            _apiHistory.Add(new Dictionary<string, string> {
                { "role", "user" },
                { "content", "您好" }
            });
            await AiChatHelper.SendStreamChatAsync(_apiHistory, (token) =>
            {
                // 在 UI 线程更新文本
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //if (aiMsg.Content == "思考中...") aiMsg.Content = string.Empty;
                    //aiMsg.Content += token;

                });
            }, modelname);
        }
    }
    // UI 绑定数据模型
    public class ChatMessageUI : ViewModelBase
    {
        private string _content = string.Empty;
        public string Role { get; set; } = "user";

        public bool IsUser { get; set; } = false;

        public string Content
        {
            get => _content;
            set {SetProperty(ref _content, value);  }
        }

        public Visibility AiVisibility => Role == "ai" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UserVisibility => Role == "user" ? Visibility.Visible : Visibility.Collapsed;


    }
}
