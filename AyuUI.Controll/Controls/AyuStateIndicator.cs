using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AyuUI.Controll.Controls
{
    /// <summary>
    /// 状态机指示灯状态枚举
    /// </summary>
    public enum IndicatorState
    {
        /// <summary>
        /// 离线 / 未连接 (深灰色，无光晕)
        /// </summary>
        Offline,

        /// <summary>
        /// 就绪 / 待机 (科技蓝，微光)
        /// </summary>
        Idle,

        /// <summary>
        /// 运行中 / 正常 (翡翠绿，常亮光晕)
        /// </summary>
        Running,

        /// <summary>
        /// 警告 / 维护中 (琥珀橙，慢速呼吸闪烁)
        /// </summary>
        Warning,

        /// <summary>
        /// 故障 / 急停报警 (警示红，高频呼吸闪烁)
        /// </summary>
        Error
    }

    public class AyuStateIndicator : Control
    {
        static AyuStateIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AyuStateIndicator), new FrameworkPropertyMetadata(typeof(AyuStateIndicator)));
        }

        public IndicatorState State
        {
            get => (IndicatorState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(IndicatorState), typeof(AyuStateIndicator), new PropertyMetadata(IndicatorState.Offline));

        /// <summary>
        /// 指示灯右侧显示的文字标签（为空则仅显示灯珠）
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(AyuStateIndicator), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 灯珠直径大小（默认 12px）
        /// </summary>
        public double IndicatorSize
        {
            get => (double)GetValue(IndicatorSizeProperty);
            set => SetValue(IndicatorSizeProperty, value);
        }
        public static readonly DependencyProperty IndicatorSizeProperty =
            DependencyProperty.Register(nameof(IndicatorSize), typeof(double), typeof(AyuStateIndicator), new PropertyMetadata(12.0));
    }
}
