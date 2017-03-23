using ShowcaseBridgeService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;

namespace Showcase
{
    class AppServiceBridge
    {
        private static ThreadPoolTimer _timer;

        public static AppServiceConnection Service
        {
            get
            {
                return _service;
            }
        }
        private static AppServiceConnection _service;

        public static async Task InitAsync()
        {
            _service = AppServiceConnectionFactory.GetConnection();
            _service.RequestReceived += (AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) =>
            {
                RequestReceived?.Invoke(sender, args);
            };
            _service.ServiceClosed += (AppServiceConnection sender, AppServiceClosedEventArgs args) => { Reconnect($"Service closed: {args.Status}."); };
            var status = await _service.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Reconnect($"Connection to app service failed: {status}.");
                return;
            }
            Debug.WriteLine("Connected to app service.");
        }

        private static void Reconnect(string reason)
        {
            _service = null;
            _timer = ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer timer) => { await InitAsync(); }, TimeSpan.FromSeconds(10));
            Debug.WriteLine(reason);
        }

        public static async void RequestUpdate(string key)
        {
            await SendMessageAsync(new ValueSet
            {
                [key] = null
            });
        }

        public static async void RequestUpdate(List<string> keys)
        {
            var message = new ValueSet();
            foreach (var key in keys)
            {
                message[key] = null;
            }
            await SendMessageAsync(message);
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
