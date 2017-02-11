using Microsoft.Azure.Devices.Client;
using System;
using System.Diagnostics;
using System.Text;
using Windows.Data.Json;

namespace BackgroundWeatherStation
{
    class IoTHubClient
    {
        private DeviceClient _deviceClient;

        public IoTHubClient()
        {
            _deviceClient = DeviceClient.Create(Keys.IOT_HUB_ENDPOINT, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Keys.DEVICE_ID, Keys.DEVICE_KEY));
            _deviceClient.OpenAsync();
        }

        public async void LogData(double temperature, double humidity, double pressure)
        {
            var messageString = new JsonObject
            {
                { "currentTemperature", JsonValue.CreateNumberValue(temperature) },
                { "currentHumidity", JsonValue.CreateNumberValue(humidity) },
                { "currentPressure", JsonValue.CreateNumberValue(pressure) },
                { "deviceId", JsonValue.CreateStringValue(Keys.DEVICE_ID) },
                { "time", JsonValue.CreateStringValue(DateTime.Now.ToString()) },
            }.Stringify();
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            try
            {
                await _deviceClient.SendEventAsync(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error logging data to Azure:\n" + e.Message);
            }
        }
    }
}
