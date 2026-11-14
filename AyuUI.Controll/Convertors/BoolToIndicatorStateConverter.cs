using AyuUI.Controll.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AyuUI.Controll.Convertors
{
    /// <summary>
    /// 将 bool 值转换为指示灯状态的转换器
    /// 默认：true -> Running (绿), false -> Offline (灰)
    /// 若传参 "Inverse"：true -> Offline (灰), false -> Running (绿)
    /// 若传参 "Error"：true -> Error (红), false -> Running (绿)
    /// </summary>
    public class BoolToIndicatorStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolVal)
                return IndicatorState.Offline;

            string param = parameter as string ?? string.Empty;

            if (param.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                return boolVal ? IndicatorState.Offline : IndicatorState.Running;
            }

            if (param.Equals("Error", StringComparison.OrdinalIgnoreCase))
            {
                return boolVal ? IndicatorState.Error : IndicatorState.Running;
            }

            // 默认正向：true = 运行 (Running)，false = 离线 (Offline)
            return boolVal ? IndicatorState.Running : IndicatorState.Offline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
