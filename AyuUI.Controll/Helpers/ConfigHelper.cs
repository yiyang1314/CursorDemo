using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace AyuUI.Controll.Helpers
{
    public static class ConfigHelper
    {
        /// <summary>
        /// 读取 App.config 或 Web.config 中的 <add key="" value=""/> 键值
        /// </summary>
        public static T GetAppSetting<T>(string key, T defaultValue = default!)
        {
            try
            {
                string? val = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrEmpty(val)) return defaultValue;
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 修改或写入 App.config 的配置项（注意：修改后需调用 Save 保存）
        /// </summary>
        public static void SetAppSetting(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// 快捷读取本地自定义 JSON 配置文件中的强类型配置项（例如 settings.json）
        /// </summary>
        public static T LoadJsonConfig<T>(string jsonFilePath) where T : new()
        {
            if (!File.Exists(jsonFilePath))
            {
                var defaultObj = new T();
                JsonHelper.SaveToFile(defaultObj, jsonFilePath);
                return defaultObj;
            }
            return JsonHelper.LoadFromFile<T>(jsonFilePath) ?? new T();
        }

        /// <summary>
        /// 快捷保存本地自定义 JSON 配置文件
        /// </summary>
        public static void SaveJsonConfig<T>(T configObj, string jsonFilePath)
        {
            JsonHelper.SaveToFile(configObj, jsonFilePath);
        }
    }
}
