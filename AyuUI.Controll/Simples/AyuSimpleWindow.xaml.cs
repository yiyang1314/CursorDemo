using AyuUI.Controll.Controls;
using AyuUI.Controll.IOC;
using AyuUI.Controll.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AyuUI.Controll.Simples
{
    /// <summary>
    /// AyuSimpleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AyuSimpleWindow : CustomWindow
    {
        public AyuSimpleWindow()
        {
            InitializeComponent();
            DataContext = AyuContainer.Default.Resolve<MainViewModel>();
        }

        private void SwitchStyle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && Enum.TryParse<TitleBarStyleMode>(btn.Tag?.ToString(), out var mode))
            {
                this.StyleMode = mode;
            }
        }
    }
}
