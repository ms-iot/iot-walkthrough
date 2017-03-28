using Microsoft.Azure.Devices.Shared;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Threading;

namespace BackgroundWeatherStation
{
    public sealed class StartupTask : IBackgroundTask
    {
        private WeatherStation _station = new WeatherStation();
        private IoTHubClient _client = new IoTHubClient();
        private ThreadPoolTimer _timer;
        private BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a BackgroundTaskDeferral and hold it forever if initialization is sucessful.
            _deferral = taskInstance.GetDeferral();
            try
            {
                await _station.InitAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("I2C initialization failed: " + e.Message);
                _deferral.Complete();
                return;
            }
            AppServiceBridge.RequestReceived += AppServiceRequestHandler;
            await AppServiceBridge.InitAsync();
            await _client.InitAsync();

            taskInstance.Canceled += (IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) =>
            {
                Debug.WriteLine("Cancelled: reason " + reason);
                _deferral.Complete();
            };

            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;

            _timer = ThreadPoolTimer.CreatePeriodicTimer(LogSensorDataAsync, TimeSpan.FromSeconds(5));
            LogSensorDataAsync(null);
        }

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            var level = MemoryManager.AppMemoryUsageLevel;
            if (level != AppMemoryUsageLevel.Low)
            {
                Debug.WriteLine($"Memory limit {MemoryManager.AppMemoryUsageLevel} crossed: Current: {MemoryManager.AppMemoryUsage}, limit: {MemoryManager.AppMemoryUsageLimit}");
            }
        }

        private async void LogSensorDataAsync(ThreadPoolTimer timer)
        {
            var temperature = _station.ReadTemperature();
            var humidity = _station.ReadHumidity();
            var pressure = _station.ReadPressure();

            await _client.LogDataAsync(temperature, humidity, pressure);

            ValueSet message = new ValueSet
            {
                ["temperature"] = temperature,
                ["humidity"] = humidity,
                ["pressure"] = pressure
            };
            await AppServiceBridge.SendMessageAsync(message);

            Debug.WriteLine("Logged data");
        }

        private async void AppServiceRequestHandler(AppServiceConnection connection, AppServiceRequestReceivedEventArgs args)
        {
            TwinCollection collection = null;
            foreach (var pair in args.Request.Message)
            {
                if (pair.Key.StartsWith("Config"))
                {
                    if (collection == null)
                    {
                        collection = new TwinCollection();
                    }
                    collection[pair.Key] = pair.Value;
                }
            }
            if (collection != null)
            {
                await _client.UpdateReportedPropertiesAsync(collection);
            }
        }
    }
}
