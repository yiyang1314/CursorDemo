using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AyuUI.Controll.Controls
{
    public class BaseWindow : Window
    {
        //static BaseWindow()
        //{
        //    // 覆盖默认样式键，使其在 Themes/Generic.xaml 中查找样式
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow), new FrameworkPropertyMetadata(typeof(BaseWindow)));
        //}

        public BaseWindow()
        {
            LoadWindowStyle();
            // 绑定窗口控制命令
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => SystemCommands.CloseWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => SystemCommands.MaximizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => SystemCommands.RestoreWindow(this)));

            // 监听状态变化以动态调整最大化时的边框边距（防止溢出屏幕）
            StateChanged += OnWindowStateChanged;
        }
        private void LoadWindowStyle()
        {
            try
            {
                // 这里的路径填你存放 CustomWindow 样式的真实 XAML 路径（注意：两个文件夹里选你放了样式的那个）
                var uri = new Uri("/AyuUI.Controll;component/Themes/GenericTheme.xaml", UriKind.Relative);
                var dict = Application.LoadComponent(uri) as ResourceDictionary;
                if (dict != null)
                {
                    // 将字典加入当前窗口资源
                    this.Resources.MergedDictionaries.Add(dict);
                    // 如果样式有指定 Key，直接赋值给 this.Style；如果是隐式样式，也会自动生效
                    if (dict.Contains(typeof(BaseWindow)))
                    {
                        this.Style = dict[typeof(BaseWindow)] as Style;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果路径不对，弹窗直接告诉你具体原因，不再静默黑屏！
                MessageBox.Show($"加载窗口样式失败：{ex.Message}");
            }
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // 最大化时系统会有约 7-8px 的隐藏边框溢出，这里通过 Padding 补偿
                BorderThickness = new Thickness(7);
            }
            else
            {
                BorderThickness = new Thickness(0);
            }
        }

        #region 依赖属性 (图标、自定义命令、状态显示)

        public ImageSource? AppIcon
        {
            get => (ImageSource?)GetValue(AppIconProperty);
            set => SetValue(AppIconProperty, value);
        }
        public static readonly DependencyProperty AppIconProperty =
            DependencyProperty.Register(nameof(AppIcon), typeof(ImageSource), typeof(BaseWindow), new PropertyMetadata(null));

        // 设置按钮命令
        public ICommand? SettingsCommand
        {
            get => (ICommand?)GetValue(SettingsCommandProperty);
            set => SetValue(SettingsCommandProperty, value);
        }
        public static readonly DependencyProperty SettingsCommandProperty =
            DependencyProperty.Register(nameof(SettingsCommand), typeof(ICommand), typeof(BaseWindow), new PropertyMetadata(null));

        // 主题切换命令
        public ICommand? ToggleThemeCommand
        {
            get => (ICommand?)GetValue(ToggleThemeCommandProperty);
            set => SetValue(ToggleThemeCommandProperty, value);
        }
        public static readonly DependencyProperty ToggleThemeCommandProperty =
            DependencyProperty.Register(nameof(ToggleThemeCommand), typeof(ICommand), typeof(BaseWindow), new PropertyMetadata(null));

        // 帮助按钮命令
        public ICommand? HelpCommand
        {
            get => (ICommand?)GetValue(HelpCommandProperty);
            set => SetValue(HelpCommandProperty, value);
        }
        public static readonly DependencyProperty HelpCommandProperty =
            DependencyProperty.Register(nameof(HelpCommand), typeof(ICommand), typeof(BaseWindow), new PropertyMetadata(null));

        // 中英文切换命令
        public ICommand? ToggleLanguageCommand
        {
            get => (ICommand?)GetValue(ToggleLanguageCommandProperty);
            set => SetValue(ToggleLanguageCommandProperty, value);
        }
        public static readonly DependencyProperty ToggleLanguageCommandProperty =
            DependencyProperty.Register(nameof(ToggleLanguageCommand), typeof(ICommand), typeof(BaseWindow), new PropertyMetadata(null));

        // 当前语言文本显示（默认 "中"）
        public string LanguageText
        {
            get => (string)GetValue(LanguageTextProperty);
            set => SetValue(LanguageTextProperty, value);
        }
        public static readonly DependencyProperty LanguageTextProperty =
            DependencyProperty.Register(nameof(LanguageText), typeof(string), typeof(BaseWindow), new PropertyMetadata("中"));

        // 是否为暗色主题（用于切换月亮/太阳图标）
        public bool IsDarkTheme
        {
            get => (bool)GetValue(IsDarkThemeProperty);
            set => SetValue(IsDarkThemeProperty, value);
        }
        public static readonly DependencyProperty IsDarkThemeProperty =
            DependencyProperty.Register(nameof(IsDarkTheme), typeof(bool), typeof(BaseWindow), new PropertyMetadata(true));

        #endregion
    }
}
