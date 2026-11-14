using AyuUI.Controll.IOC.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AyuUI.Controll.IOC
{
    internal sealed class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public ServiceLifetime Lifetime { get; }
        public string? Name { get; }
        public Func<AyuContainer, object>? Factory { get; }
        public object? SingletonInstance { get; set; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string? name = null)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
            Name = name;
        }

        public ServiceDescriptor(Type serviceType, Type implementationType, Func<AyuContainer, object> factory, ServiceLifetime lifetime, string? name = null)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Factory = factory;
            Lifetime = lifetime;
            Name = name;
        }
    }

    /// <summary>
    /// XAML 视图自动化注入扩展
    /// 示例 1：DataContext="{ioc:Inject vm:MainViewModel}"
    /// 示例 2：DataContext="{ioc:Inject vm:DeviceViewModel, Name=PLC_Device}"
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class InjectExtension : MarkupExtension
    {
        public Type? Type { get; set; }
        public string? Name { get; set; }

        public InjectExtension() { }
        public InjectExtension(Type type) => Type = type;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null)
                throw new InvalidOperationException("Inject 标记扩展必须指定 Type 属性。");

            return AyuContainer.Default.Resolve(Type, Name);
        }
    }
}
