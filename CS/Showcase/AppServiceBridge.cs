using ShowcaseBridgeService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Showcase
{
    class AppServiceBridge
    {
        private static AppServiceConnection _service;
        public static AppServiceConnection Service { get { return _service; } }

        public static async Task InitAsync()
        {
            if (_service == null)
            {
                _service = AppServiceConnectionFactory.GetConnection();
                var status = await _service.OpenAsync();
                // Should never fail, since app service is installed with the foreground app
                Debug.Assert(status == AppServiceConnectionStatus.Success, "Connection to app service failed");
            }
        }

        public static async void RequestUpdate(string key)
        {
            var message = new ValueSet();
            message[key] = null;
            await Service.SendMessageAsync(message);
        }
    }
}
