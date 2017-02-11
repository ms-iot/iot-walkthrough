using ShowcaseBridgeService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
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
        private AppServiceConnection _service = AppServiceConnectionFactory.GetConnection();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a BackgroundTaskDeferral and hold it forever if initialization is sucessful.
            var deferral = taskInstance.GetDeferral();
            if (!await Init())
            {
                deferral.Complete();
                return;
            }

            _service.RequestReceived += (AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) =>
            {
                Debug.WriteLine("Request callback received");
            };
            _service.ServiceClosed += (AppServiceConnection sender, AppServiceClosedEventArgs args) =>
            {
                Debug.WriteLine("Service closed: " + args.Status);
                deferral.Complete();
            };

            _timer = ThreadPoolTimer.CreatePeriodicTimer(LogSensorData, TimeSpan.FromSeconds(5));
        }

        private async Task<bool> Init()
        {
            if (!await _station.InitI2c())
            {
                Debug.WriteLine("I2C initialization failed");
                return false;
            }

            var serviceStatus = await _service.OpenAsync();
            if (serviceStatus != AppServiceConnectionStatus.Success)
            {
                Debug.WriteLine("Opening service failed: " + serviceStatus);
                return false;
            }
            return true;
        }

        private async void LogSensorData(ThreadPoolTimer timer)
        {
            var temperature = _station.ReadTemperature();
            var humidity = _station.ReadHumidity();
            var pressure = _station.ReadPressure();

            _client.LogData(temperature, humidity, pressure);

            ValueSet message = new ValueSet();
            message["temperature"] = temperature;
            message["humidity"] = humidity;
            message["pressure"] = pressure;
            await _service.SendMessageAsync(message);
        }
    }
}
