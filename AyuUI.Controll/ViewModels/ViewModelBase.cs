using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.ViewModels
{
    /// <summary>
    /// 自定义属性通知基类 (ViewModel 与 Model 均可继承)
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并在值发生变化时自动通知 UI
        /// </summary>
        /// <returns>值是否发生了实际改变</returns>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并同时触发其他关联属性的通知（如计算属性）
        /// </summary>
        protected bool SetProperty<T>(ref T field, T newValue, string[] dependentProperties, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, newValue, propertyName))
                return false;

            if (dependentProperties != null)
            {
                foreach (var prop in dependentProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            return true;
        }
        /// <summary>
        /// 设置属性值并同时触发其他关联属性的通知（如计算属性）
        /// </summary>
        protected bool Set<T>(ref T field, T newValue, string[] dependentProperties, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, newValue, propertyName))
                return false;

            if (dependentProperties != null)
            {
                foreach (var prop in dependentProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            return true;
        }
    }

    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并在值发生变化时自动通知 UI
        /// </summary>
        /// <returns>值是否发生了实际改变</returns>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并同时触发其他关联属性的通知（如计算属性）
        /// </summary>
        protected bool SetProperty<T>(ref T field, T newValue, string[] dependentProperties, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, newValue, propertyName))
                return false;

            if (dependentProperties != null)
            {
                foreach (var prop in dependentProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            return true;
        }
        /// <summary>
        /// 设置属性值并同时触发其他关联属性的通知（如计算属性）
        /// </summary>
        protected bool Set<T>(ref T field, T newValue, string[] dependentProperties, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, newValue, propertyName))
                return false;

            if (dependentProperties != null)
            {
                foreach (var prop in dependentProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            return true;
        }
    }
}
