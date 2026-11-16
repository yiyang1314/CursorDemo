using AyuUI.Controll.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AyuUI.Controll.Pages
{
    /// <summary>
    /// AyuToastContainer.xaml 的交互逻辑
    /// </summary>
    public partial class AyuToastContainer : Window
    {
        private readonly ObservableCollection<ToastModel> _activeToasts = new();
        private readonly ToastPosition _position;
        private const int MaxVisibleCount = 3; // 最大同时显示数

        public AyuToastContainer(ToastPosition position)
        {
            InitializeComponent();
            _position = position;
            ToastItemsControl.ItemsSource = _activeToasts;
            UpdateLayoutPosition();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateScreenPosition();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScreenPosition();
        }

        private void UpdateLayoutPosition()
        {
            // 根据位置调整 StackPanel 的上下对齐方式
            switch (_position)
            {
                case ToastPosition.TopRight:
                case ToastPosition.TopLeft:
                case ToastPosition.TopCenter:
                    ToastStack.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case ToastPosition.BottomRight:
                case ToastPosition.BottomLeft:
                case ToastPosition.BottomCenter:
                    ToastStack.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
            }

            switch (_position)
            {
                case ToastPosition.TopRight:
                case ToastPosition.BottomRight:
                    ToastStack.HorizontalAlignment = HorizontalAlignment.Right;
                    RootLayout.Margin = new Thickness(16, 16, 24, 16);
                    break;
                case ToastPosition.TopLeft:
                case ToastPosition.BottomLeft:
                    ToastStack.HorizontalAlignment = HorizontalAlignment.Left;
                    RootLayout.Margin = new Thickness(24, 16, 16, 16);
                    break;
                case ToastPosition.TopCenter:
                case ToastPosition.BottomCenter:
                    ToastStack.HorizontalAlignment = HorizontalAlignment.Center;
                    RootLayout.Margin = new Thickness(16);
                    break;
            }
        }

        private void UpdateScreenPosition()
        {
            var desktopWorkArea = SystemParameters.WorkArea;
            Left = desktopWorkArea.Left;
            Top = desktopWorkArea.Top;
            Width = desktopWorkArea.Width;
            Height = desktopWorkArea.Height;
        }

        public void AddToast(string title, string message, ToastType type, int durationSeconds, Action? onClick)
        {
            var model = new ToastModel
            {
                Title = title,
                Message = message,
                Type = type,
                DurationSeconds = durationSeconds,
                IconData = GetIconGeometry(type),
                OnClick = onClick
            };

            // 限制最大显示 3 个，超出的放入队列或溢出统计
            if (_activeToasts.Count >= MaxVisibleCount)
            {
                // 移除最老的一个
                var oldest = _activeToasts[0];
                oldest.Timer?.Stop();
                _activeToasts.RemoveAt(0);
            }

            _activeToasts.Add(model);
            RefreshOverflowBadge();

            // 启动定时销毁计时器
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(durationSeconds) };
            timer.Tick += (s, ev) =>
            {
                if (!model.IsPaused)
                {
                    timer.Stop();
                    RemoveToast(model);
                }
            };
            model.Timer = timer;
            timer.Start();
        }

        private void RemoveToast(ToastModel model)
        {
            if (_activeToasts.Contains(model))
            {
                model.Timer?.Stop();
                _activeToasts.Remove(model);
                RefreshOverflowBadge();

                if (_activeToasts.Count == 0)
                {
                    Close(); // 如果没有 Toast 了，自动销毁宿主窗口
                }
            }
        }

        private void RefreshOverflowBadge()
        {
            // 如果总积压或当前超过 3 个，在最后一个卡片上显示总数统计
            int total = _activeToasts.Count;
            for (int i = 0; i < total; i++)
            {
                if (i == total - 1 && total >= MaxVisibleCount)
                {
                    _activeToasts[i].BadgeVisibility = Visibility.Visible;
                    _activeToasts[i].TotalCountText = $"总计 {total}+";
                }
                else
                {
                    _activeToasts[i].BadgeVisibility = Visibility.Collapsed;
                }
            }
        }

        private string GetIconGeometry(ToastType type) => type switch
        {
            ToastType.Success => "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z",
            ToastType.Info => "M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z",
            ToastType.Warning => "M12 5.99L19.53 19H4.47L12 5.99M12 2L1 21h22L12 2zm1 14h-2v2h2v-2zm0-6h-2v4h2v-4z",
            ToastType.Error => "M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z",
            ToastType.Loading => "M12,4V2A10,10 0 0,0 2,12H4A8,8 0 0,1 12,4Z",
            _ => string.Empty
        };

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ToastModel model })
            {
                model.IsPaused = true; // 悬停暂停倒计时
            }
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ToastModel model })
            {
                model.IsPaused = false; // 移开恢复倒计时
            }
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ToastModel model })
            {
                model.OnClick?.Invoke();
                RemoveToast(model);
            }
        }
    }
}
