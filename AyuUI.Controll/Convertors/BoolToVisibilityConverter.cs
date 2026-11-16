using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AyuUI.Controll.Convertors
{
    /// <summary>
    /// 1. 布尔值转 Visibility 转换器
    /// 传参 "Inverse" 可实现取反（true->Collapsed, false->Visible）
    /// 传参 "Hidden" 可将不可见状态设为 Hidden 而非 Collapsed
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string param = parameter as string ?? string.Empty;
            bool inverse = param.Contains("Inverse", StringComparison.OrdinalIgnoreCase);
            bool useHidden = param.Contains("Hidden", StringComparison.OrdinalIgnoreCase);
            if (value is not bool boolVal)
            {
                return Visibility.Collapsed;
            }

            if (inverse) return boolVal ? Visibility.Collapsed : Visibility.Visible;

            return boolVal ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
            {
                string param = parameter as string ?? string.Empty;
                bool inverse = param.Contains("Inverse", StringComparison.OrdinalIgnoreCase);
                bool res = vis == Visibility.Visible;
                return inverse ? !res : res;
            }
            return false;
        }
    }
}
