using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Devices.Tpm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Collections;

namespace BackgroundWeatherStation
{
    class IoTHubClient
    {
        private TpmDevice _tpm = new TpmDevice(0);
        private DeviceClient _deviceClient;
        private SemaphoreSlim _deviceClientSemaphore = new SemaphoreSlim(1);
        private String _id;
        private ConcurrentQueue<Message> _pendingMessages = new ConcurrentQueue<Message>();

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
                await TryRefreshTokenAsync();
            }
        }

        public async Task LogDataAsync(double temperature, double humidity, double pressure)
        {
            if (_id == null)
            {
                Debug.WriteLine("TPM keys not provisioned, ignoring telemetry logging calls");
                return;
            }

            if (_pendingMessages.Count > 10)
            {
                Debug.WriteLine("Too many queued messages, ignoring logging call");
            }
            else
            {
                var messageString = new JsonObject
                {
                    { "currentTemperature", JsonValue.CreateNumberValue(temperature) },
                    { "currentHumidity", JsonValue.CreateNumberValue(humidity) },
                    { "currentPressure", JsonValue.CreateNumberValue(pressure) },
                    { "deviceId", JsonValue.CreateStringValue(_id) },
                    { "time", JsonValue.CreateStringValue(DateTime.Now.ToString()) },
                }.Stringify();
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                _pendingMessages.Enqueue(message);
            }

            if (await _deviceClientSemaphore.WaitAsync(0))
            {
                try
                {
                    if (_deviceClient == null && !await TryRefreshTokenAsync())
                    {
                        return;
                    }

                    if (_pendingMessages.TryPeek(out Message currentMessage))
                    {
                        await SendEventAsync(currentMessage);
                        _pendingMessages.TryDequeue(out currentMessage);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error logging data to Azure:\n" + e.Message);
                }
                finally
                {
                    _deviceClientSemaphore.Release();
                }
            }
            else
            {
                Debug.WriteLine("Already communicating with Azure, skipping send");
            }
        }

        private async Task<Task> SendEventAsync(Message message)
        {
            Task sendMessageTask = null;
            try
            {
                sendMessageTask = _deviceClient.SendEventAsync(message);
            }
            catch (UnauthorizedException)
            {
                Debug.WriteLine("Azure UnauthorizedException, refreshing SAS token");
                if (!await TryRefreshTokenAsync())
                {
                    throw new UnauthorizedException("Failed to refresh Azure connection");
                }
                sendMessageTask = _deviceClient.SendEventAsync(message);
            }
            return sendMessageTask;
        }

        private async Task<bool> TryRefreshTokenAsync()
        {
            IAuthenticationMethod method;
            try
            {
                var token = _tpm.GetSASToken();
                if (String.IsNullOrEmpty(token))
                {
                    throw new Exception("TPM generated empty token");
                }
                method = AuthenticationMethodFactory.CreateAuthenticationWithToken(_id, _tpm.GetSASToken());
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception authenticating with TPM token: " + e.Message);
                return false;
            }

            try
            {
                if (_deviceClient != null)
                {
                    await _deviceClient.CloseAsync();
                }
                _deviceClient = DeviceClient.Create(_tpm.GetHostName(), method, TransportType.Mqtt_WebSocket_Only);
                await _deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyChanged, null);
                var twin = await _deviceClient.GetTwinAsync();
                await OnDesiredPropertyChanged(twin.Properties.Desired, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception connecting to Azure: " + e.Message);
                _deviceClient = null;
                return false;
            }
            return true;
        }

        private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            ValueSet properties = new ValueSet
            {
                ["version"] = desiredProperties.Version
            };
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
