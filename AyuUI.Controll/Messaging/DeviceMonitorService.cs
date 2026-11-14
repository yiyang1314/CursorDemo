using AyuUI.Controll.IOC.Attributes;

namespace AyuUI.Controll.Messaging
{
    [Service(ServiceLifetime.Singleton)]
    public class DeviceMonitorService
    {
        private readonly IMessenger _messenger;

        // 通过 IoC 自动注入 IMessenger
        public DeviceMonitorService(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public async Task StartPollAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                // 在后台线程发布消息到 "PLC_Channel" 频道
                _messenger.Publish(new DeviceStatusMessage("PLC-01", "运行正常", 42.5), token: "PLC_Channel");
            });
        }
    }
}
