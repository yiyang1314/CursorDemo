using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AyuUI.Controll.Controls
{
    public class AyuToggleSwitch : ToggleButton
    {
        static AyuToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AyuToggleSwitch), new FrameworkPropertyMetadata(typeof(AyuToggleSwitch)));
        }

        /// <summary>
        /// 开启状态下的轨道背景色（默认科技蓝）
        /// </summary>
        public Brush CheckedBackground
        {
            get => (Brush)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }
        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register(nameof(CheckedBackground), typeof(Brush), typeof(AyuToggleSwitch),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"))));

        /// <summary>
        /// 关闭状态下的轨道背景色（默认深灰）
        /// </summary>
        public Brush UncheckedBackground
        {
            get => (Brush)GetValue(UncheckedBackgroundProperty);
            set => SetValue(UncheckedBackgroundProperty, value);
        }
        public static readonly DependencyProperty UncheckedBackgroundProperty =
            DependencyProperty.Register(nameof(UncheckedBackground), typeof(Brush), typeof(AyuToggleSwitch),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E323E"))));

        /// <summary>
        /// 开启时显示的提示文本（如 "ON" 或 "启用"）
        /// </summary>
        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }
        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register(nameof(OnText), typeof(string), typeof(AyuToggleSwitch), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 关闭时显示的提示文本（如 "OFF" 或 "禁用"）
        /// </summary>
        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }
        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register(nameof(OffText), typeof(string), typeof(AyuToggleSwitch), new PropertyMetadata(string.Empty));
    }
}
