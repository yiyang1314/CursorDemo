using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Messaging
{
    /// <summary>
    /// 订阅者接收消息时的线程调度模式
    /// </summary>
    public enum ThreadOption
    {
        /// <summary>
        /// 在发布消息的当前线程直接同步执行（性能最高）
        /// </summary>
        PublisherThread,

        /// <summary>
        /// 自动调度到 WPF UI 主线程执行（适用于直接更新 ObservableCollection 或界面属性）
        /// </summary>
        UIThread,

        /// <summary>
        /// 在后台线程池线程中异步执行（适用于耗时统计、日志落盘等，不阻塞发布者）
        /// </summary>
        BackgroundThread
    }
}
