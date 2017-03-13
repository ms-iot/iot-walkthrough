using ShowcaseBridgeService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Showcase
{
    class AppServiceBridge
    {
        private static AppServiceConnection _service;

        public static async Task InitAsync()
        {
            if (_service == null)
            {
                _service = AppServiceConnectionFactory.GetConnection();
                _service.RequestReceived += (AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) =>
                {
                    RequestReceived?.Invoke(sender, args);
                };
                _service.ServiceClosed += (AppServiceConnection sender, AppServiceClosedEventArgs args) =>
                {
                    _service = null;
                    Debug.WriteLine("Service closed: " + args.Status);
                };
                var status = await _service.OpenAsync();
                if (status != AppServiceConnectionStatus.Success)
                {
                    _service = null;
                    Debug.WriteLine("Connection to app service failed: " + status);
                }
            }
        }

        public static async void RequestUpdate(string key)
        {
            var message = new ValueSet
            {
                [key] = null
            };
            var task = _service?.SendMessageAsync(message);
            if (task != null)
            {
                await task;
            }
        }

        public static async Task SendMessageAsync(ValueSet message)
        {
            try
            {
                var task = _service?.SendMessageAsync(message);
                if (task != null)
                {
                    await task;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Sending message failed: " + e.Message);
            }
        }

        public static TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs> RequestReceived;
    }
}
