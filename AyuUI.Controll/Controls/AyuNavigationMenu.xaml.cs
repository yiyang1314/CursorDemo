using System;
using System.Collections;
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
using System.Windows.Threading;

namespace AyuUI.Controll.Controls
{
    /// <summary>
    /// AyuNavigationMenu.xaml 的交互逻辑
    /// </summary>
    public partial class AyuNavigationMenu : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AyuNavigationMenu), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(AyuNavigationMenu), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(NavigationPosition), typeof(AyuNavigationMenu), new PropertyMetadata(NavigationPosition.Left, OnPositionChanged));

        public NavigationPosition Position
        {
            get => (NavigationPosition)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(AyuNavigationMenu), new PropertyMetadata("AyuNavGroup"));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public AyuNavigationMenu()
        {
            InitializeComponent();
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AyuNavigationMenu menu)
            {
                menu.UpdateLayoutOrientation((NavigationPosition)e.NewValue);
            }
        }

        private void UpdateLayoutOrientation(NavigationPosition pos)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                var panel = FindVisualChild<StackPanel>(MenuItemsControl);
                if (panel != null)
                {
                    // Left 时为垂直排列；Top 或 Bottom 时为水平横向排列
                    panel.Orientation = (pos == NavigationPosition.Left) ? Orientation.Vertical : Orientation.Horizontal;
                }
            });
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null) return descendant;
            }
            return null;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton { DataContext: NavigationItem item })
            {
                SelectedItem = item;
                item.Command?.Execute(null);
            }
        }
    }

    /// <summary>
    /// 根据导航栏的 Position 自动计算高亮指示条应该停靠在边框的哪个方位
    /// Left -> Right (右侧边框)
    /// Top -> Bottom (底部边框)
    /// Bottom -> Top (顶部边框)
    /// </summary>
    public class PositionToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NavigationPosition pos)
            {
                return pos switch
                {
                    NavigationPosition.Left => HorizontalAlignment.Right, // 侧边栏：指示条在右侧
                    NavigationPosition.Top => HorizontalAlignment.Stretch,  // 顶栏：指示条撑满底部
                    NavigationPosition.Bottom => HorizontalAlignment.Stretch,// 底栏：指示条撑满顶部
                    _ => HorizontalAlignment.Right
                };
            }
            return HorizontalAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
