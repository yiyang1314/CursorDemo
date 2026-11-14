using AyuUI.Controll.Dialogs;
using System;
using System.Collections.Generic;
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

namespace AyuUI.Controll.Pages
{
    /// <summary>
    /// AyuMessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AyuMessageWindow : Window
    {
        public bool IsConfirmed { get; private set; }
        public string InputResult => InputBox.Text;

        public AyuMessageWindow(
            MessageDialogType type,
            string title,
            string message,
            string confirmText = "确定",
            string cancelText = "取消",
            string placeholder = "请输入内容...",
            string defaultInput = "")
        {
            InitializeComponent();

            // 自动关联当前激活的主窗口作为 Owner，确保居中于主窗口上方
            if (Application.Current?.MainWindow != null && Application.Current.MainWindow != this && Application.Current.MainWindow.IsVisible)
            {
                Owner = Application.Current.MainWindow;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ConfigureDialog(type, title, message, confirmText, cancelText, placeholder, defaultInput);
        }

        private void ConfigureDialog(MessageDialogType type, string title, string message, string confirmText, string cancelText, string placeholder, string defaultInput)
        {
            if (type == MessageDialogType.Loading)
            {
                StandardCard.Visibility = Visibility.Collapsed;
                LoadingCard.Visibility = Visibility.Visible;
                LoadingText.Text = message;
                return;
            }

            StandardCard.Visibility = Visibility.Visible;
            LoadingCard.Visibility = Visibility.Collapsed;

            TitleText.Text = title;
            MessageText.Text = message;
            ConfirmBtn.Content = confirmText;
            CancelBtn.Content = cancelText;

            switch (type)
            {
                case MessageDialogType.Info:
                    // 科技蓝主题
                    IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#172554"));
                    IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA"));
                    ConfirmBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    IconPath.Data = Geometry.Parse("M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z");
                    break;

                case MessageDialogType.Error:
                    // 警示红主题
                    IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#450A0A"));
                    IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
                    ConfirmBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    IconPath.Data = Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z");
                    break;

                case MessageDialogType.Confirm:
                    // 琥珀橙询问主题
                    IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#451A03"));
                    IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
                    ConfirmBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    CancelBtn.Visibility = Visibility.Visible;
                    IconPath.Data = Geometry.Parse("M11,18H13V16H11V18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,6A4,4 0 0,0 8,10H10A2,2 0 0,1 12,8A2,2 0 0,1 14,10C14,12 11,11.75 11,15H13C13,12.75 16,12.5 16,10A4,4 0 0,0 12,6Z");
                    break;

                case MessageDialogType.Input:
                    // 紫蓝输入主题
                    IconBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1B4B"));
                    IconPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#818CF8"));
                    ConfirmBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
                    CancelBtn.Visibility = Visibility.Visible;
                    InputBox.Visibility = Visibility.Visible;
                    InputBox.Tag = placeholder;
                    InputBox.Text = defaultInput;
                    IconPath.Data = Geometry.Parse("M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z");
                    Loaded += (_, _) =>
                    {
                        InputBox.Focus();
                        InputBox.SelectAll();
                    };
                    break;
            }
        }

        internal void UpdateLoadingText(string text) => LoadingText.Text = text;

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsConfirmed = true;
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                IsConfirmed = false;
                Close();
            }
        }
    }
}
