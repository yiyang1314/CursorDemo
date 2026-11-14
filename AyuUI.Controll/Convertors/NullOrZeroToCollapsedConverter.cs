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
    /// 2. 多功能空/零值转折叠转换器 (NullOrZeroToCollapsedConverter)
    /// 当值为 null、数字 0、空字符串、或可空类型无值时 -> Collapsed 折叠；否则 -> Visible 可见。
    /// 传参 "Inverse" 可实现取反（值为 0 或 null 时反而 Visible）。
    /// </summary>
    public class NullOrZeroToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrZero = false;

            if (value == null)
            {
                isNullOrZero = true;
            }
            else if (value is string str)
            {
                isNullOrZero = string.IsNullOrWhiteSpace(str);
            }
            else if (value is bool b)
            {
                isNullOrZero = !b;
            }
            else if (value is int i)
            {
                isNullOrZero = (i == 0);
            }
            else if (value is double d)
            {
                isNullOrZero = (d == 0.0);
            }
            else if (value is decimal dec)
            {
                isNullOrZero = (dec == 0m);
            }
            else if (value is long l)
            {
                isNullOrZero = (l == 0L);
            }
            else if (value is float f)
            {
                isNullOrZero = (f == 0f);
            }
            else
            {
                // 其他类型，如果是数字结构尝试用 Convert.ToDouble 判断
                try
                {
                    double num = System.Convert.ToDouble(value);
                    isNullOrZero = (num == 0);
                }
                catch
                {
                    isNullOrZero = false; // 默认有值
                }
            }

            string param = parameter as string ?? string.Empty;
            if (param.Contains("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                isNullOrZero = !isNullOrZero;
            }

            return isNullOrZero ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
