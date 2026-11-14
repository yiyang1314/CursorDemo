using AyuUI.Controll.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AyuUI.Controll.Dialogs
{
    public enum MessageDialogType
    {
        Info,
        Error,
        Confirm,
        Input,
        Loading
    }

    /// <summary>
    /// Loading 弹框控制句柄（支持动态修改加载文案与安全关闭）
    /// </summary>
    public sealed class LoadingController : IDisposable
    {
        private readonly AyuMessageWindow _window;
        private bool _isClosed;

        internal LoadingController(AyuMessageWindow window)
        {
            _window = window;
        }

        /// <summary>
        /// 线程安全地更新加载提示文字
        /// </summary>
        public void UpdateMessage(string newMessage)
        {
            if (_isClosed) return;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (!_isClosed)
                    _window.UpdateLoadingText(newMessage);
            });
        }

        /// <summary>
        /// 关闭 Loading 弹框
        /// </summary>
        public void Close()
        {
            if (_isClosed) return;
            _isClosed = true;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _window.Close();
            });
        }

        public void Dispose() => Close();
    }
}
