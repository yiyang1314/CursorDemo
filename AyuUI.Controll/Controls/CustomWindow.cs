using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AyuUI.Controll.Controls
{
    /// <summary>
    /// 窗口标题栏风格枚举
    /// </summary>
    public enum TitleBarStyleMode
    {
        /// <summary>
        /// 风格1：现代工具栏风格 (原图1：图标+标题 + 右侧功能按钮)
        /// </summary>
        ModernTool,

        /// <summary>
        /// 风格2：Visual Studio 风格 (图1：Logo+全菜单+搜索+项目胶囊徽章+头像)
        /// </summary>
        VisualStudio,

        /// <summary>
        /// 风格3：极简编辑器风格 (图2：侧边栏开关+搜索图标+竖线+精简菜单)
        /// </summary>
        MinimalEditor,

        /// <summary>
        /// 风格4：现代门户导航风格 (图3：多色Logo+导航菜单+主题/GitHub+高亮按钮+搜索框)
        /// </summary>
        ModernPortal
    }

    public class CustomWindow : Window
    {
        //static CustomWindow()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        //}

        public CustomWindow()
        {
            // 【关键修改 2】：在窗口实例化时，主动加载样式字典，确保 100% 命中！
            LoadWindowStyle();

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => SystemCommands.CloseWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => SystemCommands.MaximizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => SystemCommands.RestoreWindow(this)));

            StateChanged += (s, e) => BorderThickness = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
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
                    if (dict.Contains(typeof(CustomWindow)))
                    {
                        this.Style = dict[typeof(CustomWindow)] as Style;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果路径不对，弹窗直接告诉你具体原因，不再静默黑屏！
                MessageBox.Show($"加载窗口样式失败：{ex.Message}");
            }
        }

        #region 核心风格切换属性

        public TitleBarStyleMode StyleMode
        {
            get => (TitleBarStyleMode)GetValue(StyleModeProperty);
            set => SetValue(StyleModeProperty, value);
        }
        public static readonly DependencyProperty StyleModeProperty =
            DependencyProperty.Register(nameof(StyleMode), typeof(TitleBarStyleMode), typeof(CustomWindow), new PropertyMetadata(TitleBarStyleMode.ModernTool));

        #endregion

        #region 原有通用属性与命令

        public ImageSource? AppIcon { get => (ImageSource?)GetValue(AppIconProperty); set => SetValue(AppIconProperty, value); }
        public static readonly DependencyProperty AppIconProperty = DependencyProperty.Register(nameof(AppIcon), typeof(ImageSource), typeof(CustomWindow), new PropertyMetadata(null));

        public ICommand? SettingsCommand { get => (ICommand?)GetValue(SettingsCommandProperty); set => SetValue(SettingsCommandProperty, value); }
        public static readonly DependencyProperty SettingsCommandProperty = DependencyProperty.Register(nameof(SettingsCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        public ICommand? ToggleThemeCommand { get => (ICommand?)GetValue(ToggleThemeCommandProperty); set => SetValue(ToggleThemeCommandProperty, value); }
        public static readonly DependencyProperty ToggleThemeCommandProperty = DependencyProperty.Register(nameof(ToggleThemeCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        public ICommand? HelpCommand { get => (ICommand?)GetValue(HelpCommandProperty); set => SetValue(HelpCommandProperty, value); }
        public static readonly DependencyProperty HelpCommandProperty = DependencyProperty.Register(nameof(HelpCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        public ICommand? ToggleLanguageCommand { get => (ICommand?)GetValue(ToggleLanguageCommandProperty); set => SetValue(ToggleLanguageCommandProperty, value); }
        public static readonly DependencyProperty ToggleLanguageCommandProperty = DependencyProperty.Register(nameof(ToggleLanguageCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        public string LanguageText { get => (string)GetValue(LanguageTextProperty); set => SetValue(LanguageTextProperty, value); }
        public static readonly DependencyProperty LanguageTextProperty = DependencyProperty.Register(nameof(LanguageText), typeof(string), typeof(CustomWindow), new PropertyMetadata("中"));

        public bool IsDarkTheme { get => (bool)GetValue(IsDarkThemeProperty); set => SetValue(IsDarkThemeProperty, value); }
        public static readonly DependencyProperty IsDarkThemeProperty = DependencyProperty.Register(nameof(IsDarkTheme), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        #endregion

        #region 新增三种风格的扩展插槽与属性

        /// <summary>
        /// 左侧菜单/导航插槽 (用于存放 Menu 或自定义导航按钮)
        /// </summary>
        public object? TitleBarMenuContent { get => GetValue(TitleBarMenuContentProperty); set => SetValue(TitleBarMenuContentProperty, value); }
        public static readonly DependencyProperty TitleBarMenuContentProperty = DependencyProperty.Register(nameof(TitleBarMenuContent), typeof(object), typeof(CustomWindow), new PropertyMetadata(null));

        /// <summary>
        /// 项目/解决方案徽章文本 (如 VisualStudio 风格中的 "CursorWpf")
        /// </summary>
        public string ProjectBadgeText { get => (string)GetValue(ProjectBadgeTextProperty); set => SetValue(ProjectBadgeTextProperty, value); }
        public static readonly DependencyProperty ProjectBadgeTextProperty = DependencyProperty.Register(nameof(ProjectBadgeText), typeof(string), typeof(CustomWindow), new PropertyMetadata("MyProject"));

        /// <summary>
        /// 高亮行动按钮文本 (如 ModernPortal 风格中的 "Sponsor")
        /// </summary>
        public string HighlightActionText { get => (string)GetValue(HighlightActionTextProperty); set => SetValue(HighlightActionTextProperty, value); }
        public static readonly DependencyProperty HighlightActionTextProperty = DependencyProperty.Register(nameof(HighlightActionText), typeof(string), typeof(CustomWindow), new PropertyMetadata("Sponsor"));

        public ICommand? HighlightActionCommand { get => (ICommand?)GetValue(HighlightActionCommandProperty); set => SetValue(HighlightActionCommandProperty, value); }
        public static readonly DependencyProperty HighlightActionCommandProperty = DependencyProperty.Register(nameof(HighlightActionCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        /// <summary>
        /// 搜索框文本与命令
        /// </summary>
        public string SearchText { get => (string)GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(CustomWindow), new PropertyMetadata(string.Empty));

        public ICommand? SearchCommand { get => (ICommand?)GetValue(SearchCommandProperty); set => SetValue(SearchCommandProperty, value); }
        public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        /// <summary>
        /// 侧边栏切换命令 (MinimalEditor 风格左侧按钮)
        /// </summary>
        public ICommand? ToggleSidebarCommand { get => (ICommand?)GetValue(ToggleSidebarCommandProperty); set => SetValue(ToggleSidebarCommandProperty, value); }
        public static readonly DependencyProperty ToggleSidebarCommandProperty = DependencyProperty.Register(nameof(ToggleSidebarCommand), typeof(ICommand), typeof(CustomWindow), new PropertyMetadata(null));

        /// <summary>
        /// 用户头像图片 (VisualStudio 风格右侧圆形头像)
        /// </summary>
        public ImageSource? UserAvatar { get => (ImageSource?)GetValue(UserAvatarProperty); set => SetValue(UserAvatarProperty, value); }
        public static readonly DependencyProperty UserAvatarProperty = DependencyProperty.Register(nameof(UserAvatar), typeof(ImageSource), typeof(CustomWindow), new PropertyMetadata(null));

        #endregion
    }
}