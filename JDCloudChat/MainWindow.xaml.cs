using AyuUI.Controll.Controls;
using AyuUI.Controll.IOC;
using JDCloudChat.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JDCloudChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = AyuContainer.Default.Resolve<MainWindowViewModel>();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class UserToAvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isUser = (bool)value;
            return isUser ? "我" : "AI";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
