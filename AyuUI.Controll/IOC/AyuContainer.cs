using AyuUI.Controll.IOC.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AyuUI.Controll.IOC
{
    /// <summary>
    /// 轻量级自动化注解依赖注入容器（支持同类型多名称实例、工厂创建、属性/构造函数注入）
    /// </summary>
    public sealed class AyuContainer
    {
        /// <summary>
        /// 全局默认容器单例
        /// </summary>
        public static AyuContainer Default { get; } = new AyuContainer();

        // 核心存储字典：以 (服务类型, 实例名称) 作为复合键，支持同一接口/同一派生类共存多个命名实例
        private readonly ConcurrentDictionary<(Type ServiceType, string? Name), ServiceDescriptor> _descriptors = new();
        private readonly object _singletonLock = new();

        public AyuContainer()
        {
            // 注册容器自身
            RegisterInstance(this);
        }

        #region 1. 基础类型注册 (默认无名称)

        public AyuContainer RegisterSingleton<TService, TImpl>() where TImpl : class, TService
            => RegisterInternal(typeof(TService), typeof(TImpl), ServiceLifetime.Singleton, null);

        public AyuContainer RegisterSingleton<TImpl>() where TImpl : class
            => RegisterInternal(typeof(TImpl), typeof(TImpl), ServiceLifetime.Singleton, null);

        public AyuContainer RegisterTransient<TService, TImpl>() where TImpl : class, TService
            => RegisterInternal(typeof(TService), typeof(TImpl), ServiceLifetime.Transient, null);

        public AyuContainer RegisterTransient<TImpl>() where TImpl : class
            => RegisterInternal(typeof(TImpl), typeof(TImpl), ServiceLifetime.Transient, null);

        public AyuContainer RegisterInstance<TService>(TService instance) where TService : class
            => RegisterInstanceInternal(typeof(TService), instance, null);

        #endregion

        #region 2. 同类型多实例按名称注册 (支持相同接口 + 相同派生类)

        /// <summary>
        /// 按名称注册单例实现类
        /// </summary>
        public AyuContainer RegisterSingleton<TService, TImpl>(string name) where TImpl : class, TService
        {
            ValidateName(name);
            return RegisterInternal(typeof(TService), typeof(TImpl), ServiceLifetime.Singleton, name);
        }

        /// <summary>
        /// 按名称注册单例工厂（适用于同一实现类需要传入不同构造参数的场景）
        /// </summary>
        public AyuContainer RegisterSingleton<TService, TImpl>(string name, Func<AyuContainer, TImpl> factory)
            where TImpl : class, TService
        {
            ValidateName(name);
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImpl), factory, ServiceLifetime.Singleton, name);
            _descriptors[(typeof(TService), name)] = descriptor;
            _descriptors[(typeof(TImpl), name)] = descriptor;
            return this;
        }

        /// <summary>
        /// 按名称注册瞬态工厂（每次解析按名称生成独立配置的新实例）
        /// </summary>
        public AyuContainer RegisterTransient<TService, TImpl>(string name, Func<AyuContainer, TImpl> factory)
            where TImpl : class, TService
        {
            ValidateName(name);
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var descriptor = new ServiceDescriptor(typeof(TService), typeof(TImpl), factory, ServiceLifetime.Transient, name);
            _descriptors[(typeof(TService), name)] = descriptor;
            _descriptors[(typeof(TImpl), name)] = descriptor;
            return this;
        }

        /// <summary>
        /// 按名称直接注册已创建的同类型实例
        /// </summary>
        public AyuContainer RegisterInstance<TService>(string name, TService instance) where TService : class
        {
            ValidateName(name);
            return RegisterInstanceInternal(typeof(TService), instance, name);
        }

        #endregion

        #region 3. 注解自动化扫描注册

        /// <summary>
        /// 自动扫描指定程序集中所有标记了 [Service] 特性的类并注册
        /// </summary>
        public AyuContainer RegisterFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsDefined(typeof(ServiceAttribute), false));

            foreach (var implType in types)
            {
                var attr = implType.GetCustomAttribute<ServiceAttribute>()!;
                string? name = attr.Name;

                // 1. 注册实现类自身
                _descriptors[(implType, name)] = new ServiceDescriptor(implType, implType, attr.Lifetime, name);

                // 2. 若指定了 ServiceType 则注册映射；否则自动注册其实现的所有非系统接口
                if (attr.ServiceType != null)
                {
                    if (!attr.ServiceType.IsAssignableFrom(implType))
                        throw new InvalidOperationException($"类 {implType.Name} 未实现指定的服务类型 {attr.ServiceType.Name}");

                    _descriptors[(attr.ServiceType, name)] = new ServiceDescriptor(attr.ServiceType, implType, attr.Lifetime, name);
                }
                else
                {
                    var interfaces = implType.GetInterfaces()
                        .Where(i => i != typeof(IDisposable) && (i.Namespace == null || !i.Namespace.StartsWith("System")));

                    foreach (var iface in interfaces)
                    {
                        _descriptors[(iface, name)] = new ServiceDescriptor(iface, implType, attr.Lifetime, name);
                    }
                }
            }
            return this;
        }

        public AyuContainer RegisterFromCallingAssembly() => RegisterFromAssembly(Assembly.GetCallingAssembly());

        #endregion

        #region 4. 服务解析与成员注入

        public T Resolve<T>(string? name = null) where T : class
            => (T)Resolve(typeof(T), name, new HashSet<(Type, string?)>());

        public object Resolve(Type serviceType, string? name = null)
            => Resolve(serviceType, name, new HashSet<(Type, string?)>());

        /// <summary>
        /// 获取指定接口或基类下已注册的所有命名实例集合
        /// </summary>
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            var serviceType = typeof(T);
            var keys = _descriptors.Keys.Where(k => k.ServiceType == serviceType).ToList();
            foreach (var key in keys)
            {
                yield return (T)Resolve(key.ServiceType, key.Name, new HashSet<(Type, string?)>());
            }
        }

        /// <summary>
        /// 对外部已创建的对象（如 Window / UserControl）执行 [Inject] 属性和字段注入
        /// </summary>
        public void InjectProperties(object target)
        {
            if (target == null) return;
            InjectMembers(target, new HashSet<(Type, string?)>());
        }

        private object Resolve(Type serviceType, string? name, HashSet<(Type, string?)> resolutionChain)
        {
            var key = (serviceType, name);

            if (!resolutionChain.Add(key))
            {
                var chain = string.Join(" -> ", resolutionChain.Select(k => $"{k.Item1.Name}[{k.Item2 ?? "default"}]"));
                throw new InvalidOperationException($"IoC 容器检测到循环依赖: {chain} -> {serviceType.Name}[{name ?? "default"}]");
            }

            try
            {
                if (!_descriptors.TryGetValue(key, out var descriptor))
                {
                    // 未注册的普通具体类（且未指定名称），尝试自动构造并注入其依赖
                    if (name == null && serviceType.IsClass && !serviceType.IsAbstract)
                    {
                        return CreateInstance(serviceType, resolutionChain);
                    }

                    throw new InvalidOperationException($"未找到服务 [{serviceType.FullName}] (名称: {name ?? "default"}) 的注册信息。");
                }

                if (descriptor.Lifetime == ServiceLifetime.Transient)
                {
                    return descriptor.Factory != null
                        ? descriptor.Factory(this)
                        : CreateInstance(descriptor.ImplementationType, resolutionChain);
                }

                if (descriptor.SingletonInstance == null)
                {
                    lock (_singletonLock)
                    {
                        if (descriptor.SingletonInstance == null)
                        {
                            descriptor.SingletonInstance = descriptor.Factory != null
                                ? descriptor.Factory(this)
                                : CreateInstance(descriptor.ImplementationType, resolutionChain);
                        }
                    }
                }

                return descriptor.SingletonInstance;
            }
            finally
            {
                resolutionChain.Remove(key);
            }
        }

        private object CreateInstance(Type implementationType, HashSet<(Type, string?)> resolutionChain)
        {
            var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
                throw new InvalidOperationException($"类型 {implementationType.FullName} 不存在公共构造函数。");

            // 优先选用标记了 [Inject] 的构造函数，否则选择参数最多的构造函数
            var ctor = constructors.FirstOrDefault(c => c.IsDefined(typeof(InjectAttribute), false))
                       ?? constructors.OrderByDescending(c => c.GetParameters().Length).First();

            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var injectAttr = param.GetCustomAttribute<InjectAttribute>();
                string? paramName = injectAttr?.Name;

                try
                {
                    args[i] = Resolve(param.ParameterType, paramName, resolutionChain);
                }
                catch when (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
            }

            var instance = ctor.Invoke(args);
            InjectMembers(instance, resolutionChain);
            return instance;
        }

        private void InjectMembers(object instance, HashSet<(Type, string?)> resolutionChain)
        {
            var type = instance.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // 1. 属性注入
            foreach (var prop in type.GetProperties(flags))
            {
                if (prop.CanWrite && prop.IsDefined(typeof(InjectAttribute), true))
                {
                    var attr = prop.GetCustomAttribute<InjectAttribute>();
                    var value = Resolve(prop.PropertyType, attr?.Name, resolutionChain);
                    prop.SetValue(instance, value);
                }
            }

            // 2. 字段注入
            foreach (var field in type.GetFields(flags))
            {
                if (field.IsDefined(typeof(InjectAttribute), true))
                {
                    var attr = field.GetCustomAttribute<InjectAttribute>();
                    var value = Resolve(field.FieldType, attr?.Name, resolutionChain);
                    field.SetValue(instance, value);
                }
            }
        }

        private AyuContainer RegisterInternal(Type serviceType, Type implType, ServiceLifetime lifetime, string? name)
        {
            var descriptor = new ServiceDescriptor(serviceType, implType, lifetime, name);
            _descriptors[(serviceType, name)] = descriptor;
            _descriptors[(implType, name)] = descriptor;
            return this;
        }

        private AyuContainer RegisterInstanceInternal(Type serviceType, object instance, string? name)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var descriptor = new ServiceDescriptor(serviceType, instance.GetType(), ServiceLifetime.Singleton, name)
            {
                SingletonInstance = instance
            };
            _descriptors[(serviceType, name)] = descriptor;
            _descriptors[(instance.GetType(), name)] = descriptor;
            return this;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("按名称注册时，实例名称不能为空。", nameof(name));
        }

        #endregion
    }
}
