using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AyuUI.Controll.Controls
{
    public class ShortcutPart
    {
        public string Text { get; set; } = string.Empty;
        public bool IsPlus { get; set; }
    }

    /// <summary>
    /// 将 "Ctrl+Shift+S" 转换为 [Ctrl] [+] [Shift] [+] [S] 集合，以渲染按键胶囊效果
    /// </summary>
    public class ShortcutKeyConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string gesture || string.IsNullOrWhiteSpace(gesture))
                return null;

            var keys = gesture.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(k => k.Trim())
                              .ToArray();

            var result = new List<ShortcutPart>();
            for (int i = 0; i < keys.Length; i++)
            {
                result.Add(new ShortcutPart { Text = keys[i], IsPlus = false });
                if (i < keys.Length - 1)
                {
                    result.Add(new ShortcutPart { Text = "+", IsPlus = true });
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
