using System;
using System.Net.Http;
using Windows.ApplicationModel.AppService;
using Windows.Data.Json;
using Windows.System.Threading;

namespace Showcase
{
    class OpenWeatherMap
    {
        private const String ENDPOINT = "http://api.openweathermap.org/data/2.5/weather";
        private ThreadPoolTimer _timer;
        private object _timerLock = new object();
        // Properties
        private string _zip;
        private string _country;
        private string _key;
        private bool _started;

        public class WeatherUpdateEventArgs : EventArgs
        {
            private WeatherModel _updatedWeather;

            public WeatherUpdateEventArgs(WeatherModel updatedWeather)
            {
                _updatedWeather = updatedWeather;
            }

            public WeatherModel UpdatedWeather { get { return _updatedWeather; } }
        }

        public OpenWeatherMap()
        {
            AppServiceBridge.RequestReceived += PropertyUpdate;
            AppServiceBridge.RequestUpdate("OpenWeatherMapKey");
            AppServiceBridge.RequestUpdate("OpenWeatherMapZip");
            AppServiceBridge.RequestUpdate("OpenWeatherMapCountry");
        }

        public void Start()
        {
            _started = true;
            InitTimer();
        }

        public void Stop()
        {
            _started = false;
            lock (_timerLock)
            {
                if (_timer != null)
                {
                    _timer.Cancel();
                }
            }
        }

        private void InitTimer()
        {
            lock (_timerLock)
            {
                if (_started && _timer == null && _zip != null && _country != null && _key != null)
                {
                    _timer = ThreadPoolTimer.CreatePeriodicTimer((ThreadPoolTimer timer) =>
                    {
                        FetchWeather();
                    }, TimeSpan.FromMinutes(5));
                    FetchWeather();
                }
            }
        }

        private bool TryUpdate(AppServiceRequestReceivedEventArgs args, string key, ref string output)
        {
            args.Request.Message.TryGetValue(key, out object value);
            if (value != null)
            {
                output = (string)value;
                return true;
            }
            return false;
        }

        private void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // If some value was changed, set timer to update weather
            if (TryUpdate(args, "OpenWeatherMapZip", ref _zip) | TryUpdate(args, "OpenWeatherMapCountry", ref _country) | TryUpdate(args, "OpenWeatherMapKey", ref _key))
            {
                InitTimer();
            }
        }

        private async void FetchWeather()
        {
            JsonObject json = await new HttpHelper(BuildRequest()).GetJsonAsync();
            if (json == null)
            {
                return;
            }

            JsonObject mainJson = json.GetNamedObject("main");
            JsonObject weatherJson = json.GetNamedArray("weather").GetObjectAt(0);
            string description = weatherJson.GetNamedString("main") + " - " + weatherJson.GetNamedString("description");
            var weather = new WeatherModel(mainJson.GetNamedNumber("temp") - 273.15, mainJson.GetNamedNumber("humidity"),
                mainJson.GetNamedNumber("pressure") * 100, description, String.Format("http://openweathermap.org/img/w/{0}.png", weatherJson.GetNamedString("icon")));
            WeatherUpdate?.Invoke(this, new WeatherUpdateEventArgs(weather));
        }

        private HttpRequestMessage BuildRequest()
        {
            Uri uri = new Uri(String.Format("{0}?zip={1},{2}&appid={3}", ENDPOINT, _zip, _country, _key));
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            return req;
        }

        public EventHandler WeatherUpdate;
    }
}
