using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    /// <summary>
    /// 日志辅助类
    /// </summary>
    public static class LoggerHelper
    {
        // 日志文件保存的根目录（默认保存在程序运行目录下的 Logs 文件夹中）
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // 线程同步锁，确保多线程并发写日志时不会发生文件占用冲突
        private static readonly object LockObj = new object();

        static LoggerHelper()
        {
            // 初始化时自动创建日志目录
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// 记录普通信息日志
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void Warn(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// 记录错误日志（支持传入 Exception 自动解析堆栈信息）
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            if (ex != null)
            {
                sb.AppendLine($"   [Exception Type]: {ex.GetType().FullName}");
                sb.AppendLine($"   [Message]: {ex.Message}");
                sb.AppendLine($"   [StackTrace]: {ex.StackTrace}");
            }
            WriteLog("ERROR", sb.ToString());
        }

        /// <summary>
        /// 核心写日志方法
        /// </summary>
        private static void WriteLog(string level, string message)
        {
            string timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logLine = $"[{timeStr}] [{level}] {message}";

            // 1. 在 Visual Studio 输出窗口打印（带不同颜色区分）
            PrintToConsole(level, logLine);

            // 2. 异步或加锁写入本地文件（按天归档，每天一个日志文件）
            try
            {
                string fileName = $"Log_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(LogDirectory, fileName);

                lock (LockObj)
                {
                    File.AppendAllText(filePath, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // 防止日志系统本身崩溃导致程序异常
                System.Diagnostics.Debug.WriteLine($"[LoggerHelper Error] 写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 控制台美化输出
        /// </summary>
        private static void PrintToConsole(string level, string logLine)
        {
#if DEBUG
            var oldColor = Console.ForegroundColor;
            switch (level)
            {
                case "INFO":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "WARN":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "ERROR":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            System.Diagnostics.Debug.WriteLine(logLine);
            Console.ForegroundColor = oldColor;
#endif
        }
    }
}
