using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.IOC.Attributes
{
    /// <summary>
    /// 服务生命周期枚举
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// 单例模式（容器内全局唯一实例）
        /// </summary>
        Singleton,

        /// <summary>
        /// 瞬态模式（每次解析或注入时创建全新实例）
        /// </summary>
        Transient
    }

    /// <summary>
    /// 类级别注解：标记该类在程序集扫描时自动注册到 IoC 容器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 显式指定注册暴露的接口或基类类型（不填则自动注册自身及所有非系统接口）
        /// </summary>
        public Type? ServiceType { get; set; }

        /// <summary>
        /// 实例名称（用于区分同一类型的多个不同实例）
        /// </summary>
        public string? Name { get; set; }

        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Lifetime = lifetime;
        }
    }

    /// <summary>
    /// 成员级别注解：标记构造函数、构造函数参数、属性或字段由 IoC 容器自动注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// 指定要注入的命名实例标识（为空则注入默认无名实例）
        /// </summary>
        public string? Name { get; }

        public InjectAttribute() { }

        public InjectAttribute(string name)
        {
            Name = name;
        }
    }
}
