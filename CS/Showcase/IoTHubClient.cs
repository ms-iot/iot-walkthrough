using Microsoft.Azure.Devices.Client;
using System;
using System.Diagnostics;
using System.Text;
using Windows.Data.Json;

namespace Showcase
{
    class IoTHubClient
    {
        private DeviceClient deviceClient;

        public IoTHubClient()
        {
            deviceClient = DeviceClient.Create(Keys.AZURE_IOT_HUB_ENDPOINT,
                AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(Keys.AZURE_IOT_HUB_ENDPOINT_DEVICE_ID, Keys.AZURE_IOT_HUB_ENDPOINT_DEVICE_KEY));
        }

        public async void LogData(double temperature, double humidity)
        {
            var messageString = new JsonObject
            {
                { "currentHumidity", JsonValue.CreateNumberValue(humidity) },
                { "currentTemperature", JsonValue.CreateNumberValue(temperature) },
                { "deviceId", JsonValue.CreateStringValue(Keys.AZURE_IOT_HUB_ENDPOINT_DEVICE_ID) },
                { "time", JsonValue.CreateStringValue(DateTime.Now.ToString()) },
            }.Stringify();
            Debug.WriteLine("Sending " + messageString);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            try
            {
                await deviceClient.SendEventAsync(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
