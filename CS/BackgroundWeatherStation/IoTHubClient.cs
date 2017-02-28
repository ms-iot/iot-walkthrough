using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Devices.Tpm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Collections;

namespace BackgroundWeatherStation
{
    class IoTHubClient
    {
        private TpmDevice _tpm = new TpmDevice(0);
        private DeviceClient _deviceClient;
        private String _id;

        public async Task InitAsync()
        {
            _id = _tpm.GetDeviceId();
            if (String.IsNullOrEmpty(_id))
            {
                Debug.WriteLine("TPM keys not provisioned, ignoring Azure calls");
                _id = null;
            }
            else
            {
                await RefreshToken();
            }
        }

        public async void LogDataAsync(double temperature, double humidity, double pressure)
        {
            if (_id == null)
            {
                Debug.WriteLine("TPM keys not provisioned, ignoring telemetry logging calls");
                return;
            }
            if (_deviceClient == null && !await RefreshToken())
            {
                return;
            }
            var messageString = new JsonObject
            {
                { "currentTemperature", JsonValue.CreateNumberValue(temperature) },
                { "currentHumidity", JsonValue.CreateNumberValue(humidity) },
                { "currentPressure", JsonValue.CreateNumberValue(pressure) },
                { "deviceId", JsonValue.CreateStringValue(_id) },
                { "time", JsonValue.CreateStringValue(DateTime.Now.ToString()) },
            }.Stringify();
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            try
            {
                try
                {
                    await _deviceClient.SendEventAsync(message);
                }
                catch (UnauthorizedException)
                {
                    Debug.WriteLine("Azure UnauthorizedException, refreshing SAS token");
                    await RefreshToken();
                    await _deviceClient.SendEventAsync(message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error logging data to Azure:\n" + e.Message);
            }
        }

        private async Task<bool> RefreshToken()
        {
            var method = AuthenticationMethodFactory.CreateAuthenticationWithToken(_id, _tpm.GetSASToken());
            Twin twin;

            try
            {
                _deviceClient = DeviceClient.Create(_tpm.GetHostName(), method, TransportType.Mqtt);
                await _deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyChanged, null);
                twin = await _deviceClient.GetTwinAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception connecting to Azure: " + e.Message);
                _deviceClient = null;
                return false;
            }
            // FIXME race condition
            await OnDesiredPropertyChanged(twin.Properties.Desired, null);
            return true;
        }

        private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            ValueSet properties = new ValueSet();
            foreach (var prop in desiredProperties)
            {
                var pair = (KeyValuePair<string, object>)prop;
                var value = pair.Value as JValue;
                if (value == null)
                {
                    Debug.WriteLine("Twin key " + pair.Key + " has unsupported type");
                    continue;
                }
                properties.Add(pair.Key, pair.Value.ToString());
            }
            await AppServiceBridge.SendMessageAsync(properties);
        }
    }
}
