using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Messaging
{
    /// <summary>
    /// 主题变更通知消息
    /// </summary>
    public class ThemeChangedMessage
    {
        public bool IsDarkTheme { get; }

        public ThemeChangedMessage(bool isDarkTheme)
        {
            IsDarkTheme = isDarkTheme;
        }
    }

    /// <summary>
    /// 设备状态上报消息（演示后台线程发布 -> UI 线程接收）
    /// </summary>
    public class DeviceStatusMessage
    {
        public string DeviceName { get; }
        public string Status { get; }
        public double Temperature { get; }

        public DeviceStatusMessage(string deviceName, string status, double temperature)
        {
            DeviceName = deviceName;
            Status = status;
            Temperature = temperature;
        }
    }
}
