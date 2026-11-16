using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 防止中文被转义成 \uXXXX
            WriteIndented = false
        };

        private static readonly JsonSerializerOptions PrettyOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true // 格式化缩进
        };

        /// <summary>
        /// 对象序列化为 JSON 字符串
        /// </summary>
        public static string Serialize<T>(T obj, bool pretty = false)
        {
            if (obj == null) return string.Empty;
            return JsonSerializer.Serialize(obj, pretty ? PrettyOptions : DefaultOptions);
        }

        /// <summary>
        /// JSON 字符串反序列化为对象
        /// </summary>
        public static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            try
            {
                return JsonSerializer.Deserialize<T>(json, DefaultOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 将对象保存为 JSON 文件
        /// </summary>
        public static void SaveToFile<T>(T obj, string filePath, bool pretty = true)
        {
            var json = Serialize(obj, pretty);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 从 JSON 文件读取并反序列化为对象
        /// </summary>
        public static T? LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return Deserialize<T>(json);
        }
    }
}
