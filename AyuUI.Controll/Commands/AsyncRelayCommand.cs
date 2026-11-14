using AyuUI.Controll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AyuUI.Controll.Commands
{
    /// <summary>
    /// 无参异步命令（内置防重复点击与异常安全处理）
    /// </summary>
    public class AsyncRelayCommand : ObservableObject, ICommand
    {
        private readonly Func<CancellationToken, Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly Action<Exception>? _onException;
        private CancellationTokenSource? _cts;
        private bool _isExecuting;

        /// <summary>
        /// 当前异步任务是否正在执行（可直接绑定到 Loading 动画或按钮状态）
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (SetProperty(ref _isExecuting, value))
                {
                    RaiseCanExecuteChanged();
                }
            }
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onException = null)
            : this(_ => execute(), canExecute, onException)
        {
        }

        public AsyncRelayCommand(Func<CancellationToken, Task> execute, Func<bool>? canExecute = null, Action<Exception>? onException = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _onException = onException;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => !IsExecuting && (_canExecute == null || _canExecute());

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            IsExecuting = true;
            _cts = new CancellationTokenSource();

            try
            {
                await _execute(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                // 任务正常取消，忽略
            }
            catch (Exception ex)
            {
                if (_onException != null)
                    _onException.Invoke(ex);
                else
                    throw;
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                IsExecuting = false;
            }
        }

        /// <summary>
        /// 取消当前正在执行的异步任务
        /// </summary>
        public void Cancel() => _cts?.Cancel();

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// 带泛型参数的异步命令
    /// </summary>
    public class AsyncRelayCommand<T> : ObservableObject, ICommand
    {
        private readonly Func<T?, CancellationToken, Task> _execute;
        private readonly Predicate<T?>? _canExecute;
        private readonly Action<Exception>? _onException;
        private CancellationTokenSource? _cts;
        private bool _isExecuting;

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (SetProperty(ref _isExecuting, value))
                {
                    RaiseCanExecuteChanged();
                }
            }
        }

        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?>? canExecute = null, Action<Exception>? onException = null)
            : this((param, _) => execute(param), canExecute, onException)
        {
        }

        public AsyncRelayCommand(Func<T?, CancellationToken, Task> execute, Predicate<T?>? canExecute = null, Action<Exception>? onException = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _onException = onException;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            if (IsExecuting) return false;
            if (_canExecute == null) return true;
            if (parameter == null && typeof(T).IsValueType) return _canExecute(default);
            return _canExecute((T?)parameter);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            IsExecuting = true;
            _cts = new CancellationTokenSource();

            try
            {
                var typedParam = (parameter == null && typeof(T).IsValueType) ? default : (T?)parameter;
                await _execute(typedParam, _cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (_onException != null)
                    _onException.Invoke(ex);
                else
                    throw;
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                IsExecuting = false;
            }
        }

        public void Cancel() => _cts?.Cancel();

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
