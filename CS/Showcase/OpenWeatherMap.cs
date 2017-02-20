using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Showcase
{
    class OpenWeatherMap
    {
        private const String ENDPOINT = "http://api.openweathermap.org/data/2.5/weather";

        public async Task<WeatherModel> GetWeather()
        {
            HttpClient client = new HttpClient();
            var response = await client.SendAsync(BuildRequest());
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Open Weather Map returned error code " + response.StatusCode);
                return null;
            }
            JsonObject json;
            try
            {
                json = JsonObject.Parse(await response.Content.ReadAsStringAsync());
            }
            catch (COMException e)
            {
                Debug.WriteLine("Parsing JSON failed: " + e.Message);
                return null;
            }
            JsonObject mainJson = json.GetNamedObject("main");
            JsonObject weatherJson = json.GetNamedArray("weather").GetObjectAt(0);
            string description = weatherJson.GetNamedString("main") + " - " + weatherJson.GetNamedString("description");
            return new WeatherModel(mainJson.GetNamedNumber("temp") - 273.15, mainJson.GetNamedNumber("humidity"),
                mainJson.GetNamedNumber("pressure") * 100, description, String.Format("http://openweathermap.org/img/w/{0}.png", weatherJson.GetNamedString("icon")));
        }

        private HttpRequestMessage BuildRequest()
        {
            // TODO Add ZIP code setting
            Uri uri = new Uri(String.Format("{0}?zip={1}&appid={2}", ENDPOINT, "98052,us", Keys.OPEN_WEATHER_MAP));
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            return req;
        }
    }
}
