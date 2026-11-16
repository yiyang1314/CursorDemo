using AyuUI.Controll.IOC.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Messaging
{
    public interface IMessenger
    {
        void Subscribe<TMessage>(object recipient, Action<TMessage> action, ThreadOption threadOption = ThreadOption.PublisherThread);
        void Subscribe<TMessage>(object recipient, string token, Action<TMessage> action, ThreadOption threadOption = ThreadOption.PublisherThread);
        void Publish<TMessage>(TMessage message, string? token = null);
        void Unsubscribe(object recipient);
        void Unsubscribe<TMessage>(object recipient, string? token = null);
    }

    /// <summary>
    /// 轻量级弱引用事件聚合器（支持自动注册到 IoC 容器）
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public sealed class AyuMessenger : IMessenger
    {
        public static AyuMessenger Default { get; } = new AyuMessenger();

        private readonly ConcurrentDictionary<Type, List<ISubscription>> _subscriptions = new();
        private readonly object _lock = new();

        /// <summary>
        /// 订阅指定类型的全局消息
        /// </summary>
        public void Subscribe<TMessage>(object recipient, Action<TMessage> action, ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            SubscribeInternal(recipient, null, action, threadOption);
        }

        /// <summary>
        /// 订阅指定频道（Token）的消息
        /// </summary>
        public void Subscribe<TMessage>(object recipient, string token, Action<TMessage> action, ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            SubscribeInternal(recipient, token, action, threadOption);
        }

        /// <summary>
        /// 发布消息（支持指定频道 Token）
        /// </summary>
        public void Publish<TMessage>(TMessage message, string? token = null)
        {
            if (message == null) return;

            var messageType = typeof(TMessage);
            if (!_subscriptions.TryGetValue(messageType, out var list))
                return;

            List<ISubscription> aliveList;
            lock (_lock)
            {
                // 顺带清理已被 GC 回收的弱引用订阅者
                list.RemoveAll(s => !s.IsAlive);
                aliveList = list.Where(s => s.Token == token).ToList();
            }

            foreach (var sub in aliveList)
            {
                sub.Deliver(message);
            }
        }

        /// <summary>
        /// 取消该对象的所有消息订阅
        /// </summary>
        public void Unsubscribe(object recipient)
        {
            if (recipient == null) return;

            lock (_lock)
            {
                foreach (var list in _subscriptions.Values)
                {
                    list.RemoveAll(s => !s.IsAlive || s.Matches(recipient));
                }
            }
        }

        /// <summary>
        /// 取消该对象对特定消息类型（及指定频道）的订阅
        /// </summary>
        public void Unsubscribe<TMessage>(object recipient, string? token = null)
        {
            if (recipient == null) return;

            var messageType = typeof(TMessage);
            if (!_subscriptions.TryGetValue(messageType, out var list))
                return;

            lock (_lock)
            {
                list.RemoveAll(s => !s.IsAlive || (s.Matches(recipient) && s.Token == token));
            }
        }

        private void SubscribeInternal<TMessage>(object recipient, string? token, Action<TMessage> action, ThreadOption threadOption)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (action == null) throw new ArgumentNullException(nameof(action));

            var messageType = typeof(TMessage);
            var subscription = new WeakSubscription<TMessage>(recipient, action, token, threadOption);

            var list = _subscriptions.GetOrAdd(messageType, _ => new List<ISubscription>());
            lock (_lock)
            {
                list.RemoveAll(s => !s.IsAlive);
                list.Add(subscription);
            }
        }
    }
}
