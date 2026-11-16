using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AyuUI.Controll.Messaging
{
    internal interface ISubscription
    {
        string? Token { get; }
        bool IsAlive { get; }
        bool Matches(object recipient, Delegate? action = null);
        void Deliver(object message);
    }

    internal sealed class WeakSubscription<TMessage> : ISubscription
    {
        private readonly WeakReference? _recipientRef;
        private readonly MethodInfo _method;
        private readonly WeakReference? _targetRef;
        private readonly bool _isStatic;
        private readonly ThreadOption _threadOption;

        public string? Token { get; }

        public bool IsAlive
        {
            get
            {
                if (_recipientRef != null && !_recipientRef.IsAlive)
                    return false;
                if (!_isStatic && (_targetRef == null || !_targetRef.IsAlive))
                    return false;
                return true;
            }
        }

        public WeakSubscription(object? recipient, Action<TMessage> action, string? token, ThreadOption threadOption)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Token = token;
            _threadOption = threadOption;
            _method = action.Method;
            _isStatic = action.Target == null;

            if (recipient != null)
                _recipientRef = new WeakReference(recipient);

            if (!_isStatic)
                _targetRef = new WeakReference(action.Target);
        }

        public bool Matches(object recipient, Delegate? action = null)
        {
            if (_recipientRef == null || !ReferenceEquals(_recipientRef.Target, recipient))
                return false;

            if (action != null && action.Method != _method)
                return false;

            return true;
        }

        public void Deliver(object message)
        {
            if (!IsAlive || message is not TMessage typedMessage)
                return;

            object? target = _isStatic ? null : _targetRef?.Target;
            if (!_isStatic && target == null)
                return;

            void InvokeAction()
            {
                _method.Invoke(target, new object[] { typedMessage });
            }

            switch (_threadOption)
            {
                case ThreadOption.PublisherThread:
                    InvokeAction();
                    break;

                case ThreadOption.UIThread:
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null && !dispatcher.CheckAccess())
                    {
                        dispatcher.BeginInvoke((Action)InvokeAction);
                    }
                    else
                    {
                        InvokeAction();
                    }
                    break;

                case ThreadOption.BackgroundThread:
                    Task.Run(InvokeAction);
                    break;
            }
        }
    }
}
