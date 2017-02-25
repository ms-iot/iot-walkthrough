using ShowcaseBridgeService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace BackgroundWeatherStation
{
    class AppServiceBridge
    {
        private static AppServiceConnection _service;

        public static async Task SendMessageAsync(ValueSet message)
        {
            // FIXME race condition
            if (await TryOpenService())
            {
                await _service.SendMessageAsync(message);
            }
        }

        private static async Task<bool> TryOpenService()
        {
            if (_service == null)
            {
                Debug.WriteLine("Opening service");
                _service = AppServiceConnectionFactory.GetConnection();
                var serviceStatus = await _service.OpenAsync();
                if (serviceStatus != AppServiceConnectionStatus.Success)
                {
                    Debug.WriteLine("Opening service failed: " + serviceStatus);
                    _service = null;
                    return false;
                }
                _service.RequestReceived += (AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) =>
                {
                    Debug.WriteLine("Request callback received");
                };
                _service.ServiceClosed += (AppServiceConnection sender, AppServiceClosedEventArgs args) =>
                {
                    Debug.WriteLine("Service closed: " + args.Status);
                    _service = null;
                };
            }
            return true;
        }
    }
}
