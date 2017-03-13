using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Devices.Tpm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
        public delegate void DeviceOperation(DeviceClient client);

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
                EnqueueMessage(temperature, humidity, pressure);
            }

            if (await _deviceClientSemaphore.WaitAsync(0))
            {
                try
                {
                    await SendQueuedMessages();
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

        public async Task UpdateReportedPropertiesAsync(TwinCollection properties)
        {
            if (await _deviceClientSemaphore.WaitAsync(5000))
            {
                try
                {
                    await DoDeviceOperation(async (DeviceClient client) => { await client.UpdateReportedPropertiesAsync(properties); });
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to save settings to Azure: {e.Message}");
                }
                finally
                {
                    _deviceClientSemaphore.Release();
                }
            }

        }

        private void EnqueueMessage(double temperature, double humidity, double pressure)
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

        private async Task SendQueuedMessages()
        {
            if (_deviceClient == null && !await TryRefreshTokenAsync())
            {
                // No connection and failed to reconnect.
                return;
            }

            List<Message> messages = new List<Message>();
            while (_pendingMessages.TryDequeue(out Message message))
            {
                messages.Add(message);
            }

            try
            {
                await DoDeviceOperation(async (DeviceClient client) => { await _deviceClient.SendEventBatchAsync(messages); });
            }
            catch (Exception e)
            {
                // Re-enqueue failed events.
                foreach (var message in messages)
                {
                    _pendingMessages.Enqueue(message);
                }
                throw new Exception("SendEventBatchAsync failed: " + e.Message, e);
            }
        }

        private async Task DoDeviceOperation(DeviceOperation operation)
        {
            try
            {
                operation(_deviceClient);
            }
            catch (UnauthorizedException)
            {
                Debug.WriteLine("Azure UnauthorizedException, refreshing SAS token");
                if (!await TryRefreshTokenAsync())
                {
                    throw new UnauthorizedException("Failed to refresh Azure connection");
                }
                operation(_deviceClient);
            }
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
                await _deviceClient.OpenAsync();
                await _deviceClient.SetDesiredPropertyUpdateCallback(SendPropertyChange, null);
                var twin = await _deviceClient.GetTwinAsync();
                await SendPropertyChange(twin.Properties.Desired, null);
                await SendPropertyChange(twin.Properties.Reported, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception connecting to Azure: " + e.Message);
                _deviceClient = null;
                return false;
            }
            return true;
        }

        private async Task SendPropertyChange(IEnumerable changedProperties, object userContext)
        {
            ValueSet properties = new ValueSet();
            foreach (var prop in changedProperties)
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
