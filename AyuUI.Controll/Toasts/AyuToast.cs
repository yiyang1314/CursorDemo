using AyuUI.Controll.Pages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AyuUI.Controll.Toasts
{
    public static class AyuToast
    {
        // 按照不同的显示位置缓存对应的容器窗口，避免频繁创建销毁
        private static readonly ConcurrentDictionary<ToastPosition, AyuToastContainer> _containers = new();

        public static void Show(
            string title,
            string message,
            ToastType type = ToastType.Info,
            int durationSeconds = 3,
            ToastPosition position = ToastPosition.BottomRight,
            Action? onClick = null)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => Show(title, message, type, durationSeconds, position, onClick));
                return;
            }

            var container = _containers.GetOrAdd(position, pos =>
            {
                var win = new AyuToastContainer(pos);
                win.Closed += (s, e) => _containers.TryRemove(pos, out _);
                win.Show();
                return win;
            });

            container.AddToast(title, message, type, durationSeconds, onClick);
        }

        public static void Success(string message, string title = "成功", int duration = 3, ToastPosition pos = ToastPosition.BottomRight)
            => Show(title, message, ToastType.Success, duration, pos);

        public static void Info(string message, string title = "提示", int duration = 3, ToastPosition pos = ToastPosition.BottomRight)
            => Show(title, message, ToastType.Info, duration, pos);

        public static void Warning(string message, string title = "警告", int duration = 4, ToastPosition pos = ToastPosition.BottomRight)
            => Show(title, message, ToastType.Warning, duration, pos);

        public static void Error(string message, string title = "错误", int duration = 5, ToastPosition pos = ToastPosition.BottomRight)
            => Show(title, message, ToastType.Error, duration, pos);
    }
}
