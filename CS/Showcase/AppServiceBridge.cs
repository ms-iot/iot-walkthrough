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
                    RequestReceived(sender, args);
                };
                _service.ServiceClosed += (AppServiceConnection sender, AppServiceClosedEventArgs args) =>
                {
                    Debug.WriteLine("Service closed: " + args.Status);
                    _service = null;
                };
                var status = await _service.OpenAsync();
                // Should never fail, since app service is installed with the foreground app
                Debug.Assert(status == AppServiceConnectionStatus.Success, "Connection to app service failed");
            }
        }

        public static async void RequestUpdate(string key)
        {
            var message = new ValueSet
            {
                [key] = null
            };
            await _service.SendMessageAsync(message);
        }

        public static TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs> RequestReceived;
    }
}
