using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
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
            _client.Init();

            taskInstance.Canceled += (IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) =>
            {
                Debug.WriteLine("Cancelled: reason " + reason);
            };

            _timer = ThreadPoolTimer.CreatePeriodicTimer(LogSensorData, TimeSpan.FromSeconds(5));
        }

        private async void LogSensorData(ThreadPoolTimer timer)
        {
            var temperature = _station.ReadTemperature();
            var humidity = _station.ReadHumidity();
            var pressure = _station.ReadPressure();

            _client.LogDataAsync(temperature, humidity, pressure);

            ValueSet message = new ValueSet
            {
                ["temperature"] = temperature,
                ["humidity"] = humidity,
                ["pressure"] = pressure
            };
            await AppServiceBridge.SendMessageAsync(message);

            Debug.WriteLine("Logged data");
        }
    }
}
