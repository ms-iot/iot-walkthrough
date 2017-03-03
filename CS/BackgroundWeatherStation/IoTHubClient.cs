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
        private object _deviceClientLock = new object();

        public void Init()
        {
            _id = _tpm.GetDeviceId();
            if (String.IsNullOrEmpty(_id))
            {
                Debug.WriteLine("TPM keys not provisioned, ignoring Azure calls");
                _id = null;
            }
            else
            {
                RefreshToken();
            }
        }

        public async void LogDataAsync(double temperature, double humidity, double pressure)
        {
            if (_id == null)
            {
                Debug.WriteLine("TPM keys not provisioned, ignoring telemetry logging calls");
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

            Task sendMessageTask;
            lock (_deviceClientLock)
            {
                if (_deviceClient == null && !RefreshToken())
                {
                    return;
                }

                try
                {
                    try
                    {
                        sendMessageTask = _deviceClient.SendEventAsync(message);
                    }
                    catch (UnauthorizedException)
                    {
                        Debug.WriteLine("Azure UnauthorizedException, refreshing SAS token");
                        if (!RefreshToken())
                        {
                            return;
                        }
                        sendMessageTask = _deviceClient.SendEventAsync(message);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error logging data to Azure:\n" + e.Message);
                    return;
                }
            }

            if (sendMessageTask != null)
            {
                await sendMessageTask;
            }
        }

        private bool RefreshToken()
        {
            var method = AuthenticationMethodFactory.CreateAuthenticationWithToken(_id, _tpm.GetSASToken());

            try
            {
                if (_deviceClient != null)
                {
                    _deviceClient.CloseAsync();
                }
                _deviceClient = DeviceClient.Create(_tpm.GetHostName(), method, TransportType.Mqtt);
                SetupTwin(_deviceClient);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception connecting to Azure: " + e.Message);
                _deviceClient = null;
                return false;
            }
            return true;
        }

        private async void SetupTwin(DeviceClient deviceClient)
        {
            await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyChanged, null);
            var twin = await deviceClient.GetTwinAsync();
            await OnDesiredPropertyChanged(twin.Properties.Desired, null);
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
