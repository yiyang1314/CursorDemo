using AyuUI.Controll.IOC.Attributes;
using AyuUI.Controll.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AyuUI.Controll.Dialogs
{
    public interface IDialogService
    {
        void Info(string message, string title = "提示");
        void Error(string message, string title = "错误");
        bool Confirm(string message, string title = "确认操作");
        string? Input(string message, string title = "请输入", string placeholder = "请输入内容...", string defaultValue = "");
        LoadingController ShowLoading(string message = "正在加载中，请稍候...");
    }

    /// <summary>
    /// 弹窗服务实现（自动注册到 IoC 容器，可在 ViewModel 中直接注入 IDialogService）
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public class DialogService : IDialogService
    {
        public void Info(string message, string title = "提示") => AyuMessageBox.Info(message, title);
        public void Error(string message, string title = "错误") => AyuMessageBox.Error(message, title);
        public bool Confirm(string message, string title = "确认操作") => AyuMessageBox.Confirm(message, title);
        public string? Input(string message, string title = "请输入", string placeholder = "请输入内容...", string defaultValue = "")
            => AyuMessageBox.Input(message, title, placeholder, defaultValue);
        public LoadingController ShowLoading(string message = "正在加载中，请稍候...") => AyuMessageBox.ShowLoading(message);
    }

    /// <summary>
    /// 静态全局现代弹框调用类（线程安全，自动切回 UI 线程）
    /// </summary>
    public static class AyuMessageBox
    {
        /// <summary>
        /// 1. 信息提示框
        /// </summary>
        public static void Info(string message, string title = "系统提示", string confirmText = "知道了")
        {
            InvokeOnUI(() =>
            {
                var win = new AyuMessageWindow(MessageDialogType.Info, title, message, confirmText);
                win.ShowDialog();
            });
        }

        /// <summary>
        /// 2. 错误提示框
        /// </summary>
        public static void Error(string message, string title = "操作异常", string confirmText = "关闭")
        {
            InvokeOnUI(() =>
            {
                var win = new AyuMessageWindow(MessageDialogType.Error, title, message, confirmText);
                win.ShowDialog();
            });
        }

        /// <summary>
        /// 3. 二次确认框（返回 true 表示点击确定，false 表示取消或关闭）
        /// </summary>
        public static bool Confirm(string message, string title = "确认操作", string confirmText = "确定", string cancelText = "取消")
        {
            return InvokeOnUI(() =>
            {
                var win = new AyuMessageWindow(MessageDialogType.Confirm, title, message, confirmText, cancelText);
                win.ShowDialog();
                return win.IsConfirmed;
            });
        }

        /// <summary>
        /// 4. 输入框（确认返回输入的字符串，取消返回 null）
        /// </summary>
        public static string? Input(string message, string title = "输入信息", string placeholder = "请输入内容...", string defaultValue = "")
        {
            return InvokeOnUI(() =>
            {
                var win = new AyuMessageWindow(MessageDialogType.Input, title, message, "确定", "取消", placeholder, defaultValue);
                win.ShowDialog();
                return win.IsConfirmed ? win.InputResult : null;
            });
        }

        /// <summary>
        /// 5. 加载遮罩弹框（非阻塞，配合 using 或手动调用 Close() 关闭）
        /// </summary>
        public static LoadingController ShowLoading(string message = "正在处理中，请稍候...")
        {
            return InvokeOnUI(() =>
            {
                var win = new AyuMessageWindow(MessageDialogType.Loading, string.Empty, message);
                win.Show();
                return new LoadingController(win);
            });
        }

        private static void InvokeOnUI(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke(action);
            else
                action();
        }

        private static T InvokeOnUI<T>(Func<T> func)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                return dispatcher.Invoke(func);
            return func();
        }
    }
}
