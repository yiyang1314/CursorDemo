using AyuUI.Controll.Commands;
using AyuUI.Controll.IOC.Attributes;
using AyuUI.Controll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.IOC.Simple
{
    // 场景 A：常规单例服务（通过 [Service] 注解自动扫描注册）
    public interface ILogService
    {
        void Write(string message);
    }

    [Service(ServiceLifetime.Singleton)]
    public class LogService : ILogService
    {
        public void Write(string message) => System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    }

    // 场景 B：同一接口 + 同一派生类，但需要多个独立配置实例（如多个串口或数据库连接）
    public interface ISerialService
    {
        string PortName { get; }
        string Send(string command);
    }

    public class SerialService : ISerialService
    {
        public string PortName { get; }
        public int BaudRate { get; }

        public SerialService(string portName, int baudRate)
        {
            PortName = portName;
            BaudRate = baudRate;
        }

        public string Send(string command) => $"[{PortName} @ {BaudRate}] 执行指令 -> {command}";
    }

    [Service(ServiceLifetime.Transient)]
    public class TestViewModel : ObservableObject
    {
        private readonly ILogService _logger;
        private readonly ISerialService _plcPort;
        private readonly ISerialService _scannerPort;

        // 支持属性按名注入（甚至可以直接声明具体类 SerialService）
        [Inject("TempDebug")]
        public SerialService? DebugSerial { get; set; }

        // 构造函数混合注入：普通服务 + 同接口同派生类的不同命名实例
        public TestViewModel(
            ILogService logger,
            [Inject("PLC")] ISerialService plcPort,
            [Inject("Scanner")] ISerialService scannerPort)
        {
            _logger = logger;
            _plcPort = plcPort;
            _scannerPort = scannerPort;

            ExecuteCommand = new RelayCommand(() =>
            {
                _logger.Write("开始多设备通信...");
                var msg1 = _plcPort.Send("READ_STATUS");
                var msg2 = _scannerPort.Send("SCAN_CODE");
                var msg3 = DebugSerial?.Send("PING");

                StatusText = $"{msg1}\n{msg2}\n{msg3}";
            });
        }

        private string _statusText = "就绪";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public RelayCommand ExecuteCommand { get; }
    }
}
