using ShowcaseBridgeService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BackgroundWeatherStation
{
    class AppServiceBridge
    {
        private static AppServiceConnection _service;

        public static async Task InitAsync(TypedEventHandler<AppServiceConnection, AppServiceRequestReceivedEventArgs> handler = null)
        {
            Debug.WriteLine("Opening service");
            _service = AppServiceConnectionFactory.GetConnection();
            var serviceStatus = await _service.OpenAsync();
            // Should never fail, since app service is installed with the background app
            Debug.Assert(serviceStatus == AppServiceConnectionStatus.Success, "Opening service failed: " + serviceStatus);

            if (handler != null)
            {
                _service.RequestReceived += handler;
            }
            _service.ServiceClosed += async (AppServiceConnection sender, AppServiceClosedEventArgs args) =>
            {
                _service = null;
                Debug.WriteLine("Service closed: " + args.Status);
                await InitAsync();
            };
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
                else
                {
                    Debug.WriteLine("Service returned null task");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Sending message failed: " + e.Message);
            }
        }
    }
}
